

using Easrms.Application.DTOs.Common;
using Easrms.Application.Interfaces;
using Easrms.Domain.Entities;
using Easrms.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using OfficeOpenXml;

namespace Easrms.Infrastructure.Services.BulkUpload;

public class UserBulkUploadService : IUserBulkUploadService
{
    private readonly AppDbContext _context;

    public UserBulkUploadService(AppDbContext context)
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

        var required = new[] { "FullName", "Email", "Password", "RoleName", "ManagerEmail" };
        var headerKeys = rows.First().Keys.Select(k => k).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var missing = required.Where(r => !headerKeys.Contains(r)).ToList();
        if (missing.Any()) throw new InvalidOperationException($"Invalid file structure. Missing columns: {string.Join(", ", missing)}");

        var allowed = new HashSet<string>(required, StringComparer.OrdinalIgnoreCase);
        var unknown = headerKeys.Where(h => !allowed.Contains(h)).ToList();
        if (unknown.Any()) throw new InvalidOperationException($"Invalid file structure. Unknown columns: {string.Join(", ", unknown)}");

        var result = new BulkUploadResultDto { TotalRows = rows.Count };

        // within-file duplicates
        var emailFirstRow = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var duplicateRows = new HashSet<int>();
        for (int i = 0; i < rows.Count; i++)
        {
            var email = rows[i].GetValueOrDefault("Email")?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(email)) continue;
            var low = email.ToLowerInvariant();
            if (emailFirstRow.ContainsKey(low))
            {
                result.Errors.Add(new BulkUploadErrorDto { Row = i + 2, Identifier = email, Reason = $"Duplicate email in uploaded file (first seen on row {emailFirstRow[low] + 2})" });
                duplicateRows.Add(i);
            }
            else emailFirstRow[low] = i;
        }

        // bulk lookups
        var allRoles = await _context.Roles.ToDictionaryAsync(r => r.RoleName.ToLower(), r => r.RoleId, cancellationToken);
        var existingEmails = (await _context.Users.Select(u => u.Email.ToLower()).ToListAsync(cancellationToken)).ToHashSet();
        var allManagers = await _context.Users.Where(u => u.Role.RoleName == "Manager" && u.IsActive).ToDictionaryAsync(u => u.Email.ToLower(), u => u.UserId, cancellationToken);

        var validUsers = new List<User>();

        for (int i = 0; i < rows.Count; i++)
        {
            if (duplicateRows.Contains(i)) continue; // skip duplicates

            var row = rows[i];
            var rowNumber = i + 2;
            var errors = new List<string>();

            var fullName = row.GetValueOrDefault("FullName")?.Trim() ?? string.Empty;
            var email = row.GetValueOrDefault("Email")?.Trim() ?? string.Empty;
            var password = row.GetValueOrDefault("Password")?.Trim() ?? string.Empty;
            var roleName = row.GetValueOrDefault("RoleName")?.Trim() ?? string.Empty;
            var managerEmail = row.GetValueOrDefault("ManagerEmail")?.Trim() ?? string.Empty;

            // FIELD VALIDATION
            if (string.IsNullOrWhiteSpace(fullName) || fullName.Length > 100) errors.Add("FullName is required and must be at most 100 characters");

            try { var addr = new System.Net.Mail.MailAddress(email); if (string.IsNullOrWhiteSpace(email) || addr.Address != email) errors.Add("Email is required and must be valid"); }
            catch { errors.Add("Email is required and must be valid"); }

            if (string.IsNullOrWhiteSpace(password) || password.Length < 6) errors.Add("Password is required and must be at least 6 characters");

            if (string.IsNullOrWhiteSpace(roleName) || !allRoles.ContainsKey(roleName.ToLower())) errors.Add($"RoleName '{roleName}' is not valid. Must be one of: Admin, Manager, Employee, Support");

            if (!string.IsNullOrWhiteSpace(managerEmail)) { try { var m = new System.Net.Mail.MailAddress(managerEmail); } catch { errors.Add("ManagerEmail must be a valid email if provided"); } }

            // BUSINESS VALIDATION
            if (!errors.Any())
            {
                if (existingEmails.Contains(email.ToLower())) { result.Errors.Add(new BulkUploadErrorDto { Row = rowNumber, Identifier = email, Reason = "Email already exists in DB" }); continue; }

                Guid? managerId = null;
                if (!string.IsNullOrWhiteSpace(managerEmail))
                {
                    if (!allManagers.TryGetValue(managerEmail.ToLower(), out var mid)) { result.Errors.Add(new BulkUploadErrorDto { Row = rowNumber, Identifier = email, Reason = $"ManagerEmail '{managerEmail}' does not exist or user is not an active Manager" }); continue; }
                    else managerId = mid;
                }

                // success: create User entity
                var user = new User
                {
                    UserId = Guid.NewGuid(),
                    FullName = fullName,
                    Email = email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                    RoleId = allRoles[roleName.ToLower()],
                    ManagerId = managerId,
                    IsActive = true,
                    CreatedOn = DateTime.UtcNow
                };

                validUsers.Add(user);
            }
            else
            {
                result.Errors.Add(new BulkUploadErrorDto { Row = rowNumber, Identifier = email, Reason = string.Join("; ", errors) });
            }
        }

        // Batch insert
        if (validUsers.Any())
        {
            using var tx = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                await _context.Users.AddRangeAsync(validUsers, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
                await tx.CommitAsync(cancellationToken);
                result.InsertedCount = validUsers.Count;
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
