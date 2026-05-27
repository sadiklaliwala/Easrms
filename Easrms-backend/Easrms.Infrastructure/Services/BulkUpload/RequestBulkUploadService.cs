using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Easrms.Application.DTOs.Common;
using Easrms.Application.Interfaces;
using Easrms.Domain.Entities;
using Easrms.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using Easrms.Common.Enums;

namespace Easrms.Infrastructure.Services.BulkUpload;

public class RequestBulkUploadService : IRequestBulkUploadService
{
    private readonly AppDbContext _context;

    public RequestBulkUploadService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<BulkUploadResultDto> ProcessAsync(System.IO.Stream fileStream, string fileName, Guid employeeId, CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        if (extension != ".csv" && extension != ".xlsx")
            throw new InvalidOperationException("Only CSV and Excel (.xlsx) files are supported");

        var rows = new List<Dictionary<string, string>>();

        if (extension == ".csv")
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                IgnoreBlankLines = true,
                TrimOptions = TrimOptions.Trim,
                PrepareHeaderForMatch = args => args.Header.ToLower()
            };

            using var reader = new StreamReader(fileStream);

            using var csv = new CsvReader(reader, config);

            await csv.ReadAsync();

            csv.ReadHeader();

            while (await csv.ReadAsync())
            {
                var dict = csv.HeaderRecord.ToDictionary(
                    h => h,
                    h => csv.GetField(h) ?? string.Empty,
                    StringComparer.OrdinalIgnoreCase);

                rows.Add(dict);
            }
        }
        else
        {
            ExcelPackage.License.SetNonCommercialOrganization("EASRMS");

            using var package = new ExcelPackage(fileStream);

            var ws = package.Workbook.Worksheets.FirstOrDefault();

            if (ws == null)
                throw new InvalidOperationException("File contains no data rows");

            var header = new List<string>();

            for (int c = 1; c <= ws.Dimension.End.Column; c++)
            {
                header.Add(ws.Cells[1, c].Text);
            }

            for (int r = 2; r <= ws.Dimension.End.Row; r++)
            {
                var dict = new Dictionary<string, string>(
                    StringComparer.OrdinalIgnoreCase);

                for (int c = 1; c <= ws.Dimension.End.Column; c++)
                {
                    dict[header[c - 1]] =
                        ws.Cells[r, c].Text ?? string.Empty;
                }

                rows.Add(dict);
            }
        }

        if (!rows.Any()) throw new InvalidOperationException("File contains no data rows");

        var required = new[] { "CategoryName", "Title", "Description", "Priority" };
        var headerKeys = rows.First().Keys.Select(k => k).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var missing = required.Where(r => !headerKeys.Contains(r)).ToList();
        if (missing.Any()) throw new InvalidOperationException($"Invalid file structure. Missing columns: {string.Join(", ", missing)}");

        var allowed = new HashSet<string>(required, StringComparer.OrdinalIgnoreCase);
        var unknown = headerKeys.Where(h => !allowed.Contains(h)).ToList();
        if (unknown.Any()) throw new InvalidOperationException($"Invalid file structure. Unknown columns: {string.Join(", ", unknown)}");

        var result = new BulkUploadResultDto { TotalRows = rows.Count };

        // within-file duplicates on Title
        var titleFirstRow = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var duplicateRows = new HashSet<int>();
        for (int i = 0; i < rows.Count; i++)
        {
            var title = rows[i].GetValueOrDefault("Title")?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(title)) continue;
            var low = title.ToLowerInvariant();
            if (titleFirstRow.ContainsKey(low))
            {
                result.Errors.Add(new BulkUploadErrorDto { Row = i + 2, Identifier = title, Reason = $"Duplicate request title in uploaded file (first seen on row {titleFirstRow[low] + 2})" });
                duplicateRows.Add(i);
            }
            else titleFirstRow[low] = i;
        }

        var activeCategories = await _context.RequestCategories.Where(c => c.IsActive).ToDictionaryAsync(c => c.CategoryName.ToLower(), c => c, cancellationToken);

        var validRequests = new List<ServiceRequest>();
        var historyEntries = new List<RequestStatusHistory>();

        var priorityMap = new Dictionary<string, PriorityEnums>(StringComparer.OrdinalIgnoreCase) { { "High", PriorityEnums.High }, { "Medium", PriorityEnums.Medium }, { "Low", PriorityEnums.Low } };

        for (int i = 0; i < rows.Count; i++)
        {
            if (duplicateRows.Contains(i)) continue;

            var row = rows[i];
            var rowNumber = i + 2;
            var errors = new List<string>();

            var categoryName = row.GetValueOrDefault("CategoryName")?.Trim() ?? string.Empty;
            var title = row.GetValueOrDefault("Title")?.Trim() ?? string.Empty;
            var desc = row.GetValueOrDefault("Description")?.Trim() ?? string.Empty;
            var priority = row.GetValueOrDefault("Priority")?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(categoryName)) errors.Add("CategoryName is required");
            if (string.IsNullOrWhiteSpace(title) || title.Length > 200) errors.Add("Title is required and must be at most 200 characters");
            if (string.IsNullOrWhiteSpace(desc) || desc.Length > 1000) errors.Add("Description is required and must be at most 1000 characters");

            if (!priorityMap.ContainsKey(priority)) errors.Add("Priority must be High, Medium, or Low");

            if (!errors.Any())
            {
                if (!activeCategories.TryGetValue(categoryName.ToLower(), out var category))
                {
                    result.Errors.Add(new BulkUploadErrorDto { Row = rowNumber, Identifier = title, Reason = $"CategoryName '{categoryName}' does not exist or is not active" });
                    continue;
                }

                var req = new ServiceRequest
                {
                    RequestId = Guid.NewGuid(),
                    RequestNumber = Easrms.Common.Helpers.RequestNumberHelper.Generate(),
                    Title = title,
                    Description = desc,
                    Priority = priorityMap[priority],
                    CategoryId = category.CategoryId,
                    EmployeeId = employeeId,
                    CreatedOn = DateTime.UtcNow,
                    Status = category.IsApprovalRequired ? RequestStatusEnum.PendingApproval : RequestStatusEnum.Open
                };

                var hist = new RequestStatusHistory
                {
                    HistoryId = Guid.NewGuid(),
                    RequestId = req.RequestId,
                    OldStatus = null,
                    NewStatus = req.Status,
                    ChangedBy = employeeId,
                    ChangedOn = DateTime.UtcNow,
                    Remarks = "Created via bulk upload"
                };

                validRequests.Add(req);
                historyEntries.Add(hist);
            }
            else
            {
                result.Errors.Add(new BulkUploadErrorDto { Row = rowNumber, Identifier = title, Reason = string.Join("; ", errors) });
            }
        }

        if (validRequests.Any())
        {
            using var tx = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                await _context.ServiceRequests.AddRangeAsync(validRequests, cancellationToken);
                await _context.RequestStatusHistories.AddRangeAsync(historyEntries, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
                await tx.CommitAsync(cancellationToken);
                result.InsertedCount = validRequests.Count;
            }
            catch
            {
                await tx.RollbackAsync(cancellationToken);
                throw new InvalidOperationException("Batch insert failed. No records were inserted. Please try again.");
            }
        }

        result.FailedCount = result.Errors.Count;
        return result;
    }
}
