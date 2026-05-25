using ClosedXML.Excel;
using Easrms.Application.DTOs.Request;
using Easrms.Application.Interfaces;
using System.Drawing;

namespace Easrms.Infrastructure.Export;

public class ExcelExportService : IExportService
{
    // ── Request List → Excel ──────────────────────────────────────────────────
    public byte[] ExportRequestListToExcel(IReadOnlyList<RequestListDto> requests)
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("Requests");

        // Header row
        var headers = new[]
        {
            "Request #", "Title", "Category", "Priority",
            "Status", "Assignee", "Created On"
        };

        for (int i = 0; i < headers.Length; i++)
        {
            var cell = sheet.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#1E3A5F");
            cell.Style.Font.FontColor = XLColor.White;
        }

        // Data rows
        for (int i = 0; i < requests.Count; i++)
        {
            var r = requests[i];
            var row = i + 2;

            sheet.Cell(row, 1).Value = r.RequestNumber;
            sheet.Cell(row, 2).Value = r.Title;
            sheet.Cell(row, 3).Value = r.CategoryName;
            sheet.Cell(row, 4).Value = r.Priority.ToString();
            sheet.Cell(row, 5).Value = r.Status.ToString();
            sheet.Cell(row, 6).Value = r.AssigneeName ?? "-";
            sheet.Cell(row, 7).Value = r.CreatedOn.ToString("yyyy-MM-dd HH:mm");

            // Alternate row shading
            if (i % 2 == 1)
                sheet.Row(row).Style.Fill.BackgroundColor = XLColor.FromHtml("#F5F7FA");
        }

        sheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    // ── Request Detail → Excel ────────────────────────────────────────────────
    public byte[] ExportRequestDetailToExcel(RequestDetailDto r)
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("Request Detail");

        var fields = new List<(string Label, string Value)>
        {
            ("Request Number",  r.RequestNumber),
            ("Title",           r.Title),
            ("Description",     r.Description ?? "-"),
            ("Category",        r.CategoryName),
            ("Priority",        r.Priority.ToString()),
            ("Status",          r.Status.ToString()),
            ("Employee",        r.EmployeeName),
            ("Assignee",        r.AssigneeName ?? "-"),
            ("Created On",      r.CreatedOn.ToString("yyyy-MM-dd HH:mm")),
            ("Updated On",      r.UpdatedOn?.ToString("yyyy-MM-dd HH:mm") ?? "-"),
            ("Resolved On",     r.ResolvedOn?.ToString("yyyy-MM-dd HH:mm") ?? "-"),
            ("Closed On",       r.ClosedOn?.ToString("yyyy-MM-dd HH:mm") ?? "-"),
            ("Rejection Reason",r.RejectionReason ?? "-"),
        };

        for (int i = 0; i < fields.Count; i++)
        {
            var labelCell = sheet.Cell(i + 1, 1);
            labelCell.Value = fields[i].Label;
            labelCell.Style.Font.Bold = true;
            labelCell.Style.Fill.BackgroundColor = XLColor.FromHtml("#1E3A5F");
            labelCell.Style.Font.FontColor = XLColor.White;

            sheet.Cell(i + 1, 2).Value = fields[i].Value;
        }

        sheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    // ── PDF stubs — implemented in PdfExportService ───────────────────────────
    public byte[] ExportRequestListToPdf(IReadOnlyList<RequestListDto> requests)
        => throw new NotImplementedException("Use PdfExportService");

    public byte[] ExportRequestDetailToPdf(RequestDetailDto request)
        => throw new NotImplementedException("Use PdfExportService");
}