using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Easrms.Application.DTOs.Common;
using Easrms.Application.Interfaces;
using Easrms.Domain.Entities;
using Easrms.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;


namespace Easrms.Infrastructure.Services.BulkUpload;

public class CategoryBulkUploadService : ICategoryBulkUploadService
{
    private readonly AppDbContext _context;

    public CategoryBulkUploadService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<BulkUploadResultDto> ProcessAsync(System.IO.Stream fileStream, string fileName, CancellationToken cancellationToken = default)
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

        var required = new[] { "CategoryName", "IsApprovalRequired", "SLAHours" };
        var headerKeys = rows.First().Keys.Select(k => k).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var missing = required.Where(r => !headerKeys.Contains(r)).ToList();
        if (missing.Any()) throw new InvalidOperationException($"Invalid file structure. Missing columns: {string.Join(", ", missing)}");

        var allowed = new HashSet<string>(required, StringComparer.OrdinalIgnoreCase);
        var unknown = headerKeys.Where(h => !allowed.Contains(h)).ToList();
        if (unknown.Any()) throw new InvalidOperationException($"Invalid file structure. Unknown columns: {string.Join(", ", unknown)}");

        var result = new BulkUploadResultDto { TotalRows = rows.Count };

        // within-file duplicates
        var nameFirstRow = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var duplicateRows = new HashSet<int>();
        for (int i = 0; i < rows.Count; i++)
        {
            var name = rows[i].GetValueOrDefault("CategoryName")?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(name)) continue;
            var low = name.ToLowerInvariant();
            if (nameFirstRow.ContainsKey(low))
            {
                result.Errors.Add(new BulkUploadErrorDto { Row = i + 2, Identifier = name, Reason = $"Duplicate category name in uploaded file (first seen on row {nameFirstRow[low] + 2})" });
                duplicateRows.Add(i);
            }
            else nameFirstRow[low] = i;
        }

        var existingCategoryNames = (await _context.RequestCategories.Select(c => c.CategoryName.ToLower()).ToListAsync(cancellationToken)).ToHashSet();

        var validCategories = new List<RequestCategory>();

        for (int i = 0; i < rows.Count; i++)
        {
            if (duplicateRows.Contains(i)) continue;

            var row = rows[i];
            var rowNumber = i + 2;
            var errors = new List<string>();

            var name = row.GetValueOrDefault("CategoryName")?.Trim() ?? string.Empty;
            var isApproval = row.GetValueOrDefault("IsApprovalRequired")?.Trim() ?? string.Empty;
            var sla = row.GetValueOrDefault("SLAHours")?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(name) || name.Length > 100) errors.Add("CategoryName is required and must be at most 100 characters");

            bool isApprovalParsed = false;
            bool isApprovalValue = false;
            if (string.Equals(isApproval, "true", StringComparison.OrdinalIgnoreCase) || string.Equals(isApproval, "yes", StringComparison.OrdinalIgnoreCase) || isApproval == "1") { isApprovalParsed = true; isApprovalValue = true; }
            else if (string.Equals(isApproval, "false", StringComparison.OrdinalIgnoreCase) || string.Equals(isApproval, "no", StringComparison.OrdinalIgnoreCase) || isApproval == "0") { isApprovalParsed = true; isApprovalValue = false; }
            if (!isApprovalParsed) errors.Add("IsApprovalRequired must be true or false");

            if (!int.TryParse(sla, out var slaHours) || slaHours <= 0) errors.Add("SLAHours must be a positive integer");

            if (!errors.Any())
            {
                if (existingCategoryNames.Contains(name.ToLower())) { result.Errors.Add(new BulkUploadErrorDto { Row = rowNumber, Identifier = name, Reason = "Category name already exists in DB" }); continue; }

                var cat = new RequestCategory
                {
                    CategoryId = Guid.NewGuid(),
                    CategoryName = name,
                    IsApprovalRequired = isApprovalValue,
                    SLAHours = slaHours,
                    IsActive = true,
                    CreatedOn = DateTime.UtcNow
                };

                validCategories.Add(cat);
            }
            else
            {
                result.Errors.Add(new BulkUploadErrorDto { Row = rowNumber, Identifier = name, Reason = string.Join("; ", errors) });
            }
        }

        if (validCategories.Any())
        {
            using var tx = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                await _context.RequestCategories.AddRangeAsync(validCategories, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
                await tx.CommitAsync(cancellationToken);
                result.InsertedCount = validCategories.Count;
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
