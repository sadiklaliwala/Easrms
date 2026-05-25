using Easrms.Application.DTOs.Request;
using Easrms.Application.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.ComponentModel;
using System.Reflection.Metadata;

namespace Easrms.Infrastructure.Export;

public class PdfExportService : IExportService
{
    // ── Request List → PDF ────────────────────────────────────────────────────
    public byte[] ExportRequestListToPdf(IReadOnlyList<RequestListDto> requests)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        return QuestPDF.Fluent.Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(9));

                page.Header().Element(ComposeListHeader);
                page.Content().Element(content => ComposeListTable(content, requests));
                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Generated on ");
                    x.Span(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm"));
                    x.Span(" UTC  |  Page ");
                    x.CurrentPageNumber();
                    x.Span(" of ");
                    x.TotalPages();
                });
            });
        }).GeneratePdf();
    }

    private void ComposeListHeader(QuestPDF.Infrastructure.IContainer container)
    {
        container.PaddingBottom(10).Row(row =>
        {
            row.RelativeItem().Column(col =>
            {
                col.Item().Text("EASRMS — Request Export")
                    .FontSize(16).Bold().FontColor("#1E3A5F");
                col.Item().Text($"Total Records: {0}")
                    .FontSize(9).FontColor("#666666");
            });
        });
    }

    private void ComposeListTable(QuestPDF.Infrastructure.IContainer container, IReadOnlyList<RequestListDto> requests)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(cols =>
            {
                cols.RelativeColumn(2); // Request #
                cols.RelativeColumn(4); // Title
                cols.RelativeColumn(2); // Category
                cols.RelativeColumn(1); // Priority
                cols.RelativeColumn(2); // Status
                cols.RelativeColumn(2); // Assignee
                cols.RelativeColumn(2); // Created On
            });

            // Header
            static QuestPDF.Infrastructure.IContainer HeaderCell(QuestPDF.Infrastructure.IContainer c) =>
                c.Background("#1E3A5F").Padding(5);

            table.Header(header =>
            {
                foreach (var h in new[] { "Request #", "Title", "Category", "Priority", "Status", "Assignee", "Created On" })
                    header.Cell().Element(HeaderCell)
                        .Text(h).FontColor(Colors.White).Bold().FontSize(8);
            });

            // Rows
            bool alternate = false;
            foreach (var r in requests)
            {
                alternate = !alternate;
                string bg = alternate ? "#FFFFFF" : "#F5F7FA";

                QuestPDF.Infrastructure.IContainer DataCell(QuestPDF.Infrastructure.IContainer c) =>
                    c.Background(bg).BorderBottom(1).BorderColor("#E0E0E0").Padding(5);

                table.Cell().Element(DataCell).Text(r.RequestNumber).FontSize(8);
                table.Cell().Element(DataCell).Text(r.Title).FontSize(8);
                table.Cell().Element(DataCell).Text(r.CategoryName).FontSize(8);
                table.Cell().Element(DataCell).Text(r.Priority.ToString()).FontSize(8);
                table.Cell().Element(DataCell).Text(r.Status.ToString()).FontSize(8);
                table.Cell().Element(DataCell).Text(r.AssigneeName ?? "-").FontSize(8);
                table.Cell().Element(DataCell).Text(r.CreatedOn.ToString("yyyy-MM-dd")).FontSize(8);
            }
        });
    }

    // ── Request Detail → PDF ──────────────────────────────────────────────────
    public byte[] ExportRequestDetailToPdf(RequestDetailDto r)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        return QuestPDF.Fluent.Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().PaddingBottom(10).Text("EASRMS — Request Detail")
                    .FontSize(18).Bold().FontColor("#1E3A5F");

                page.Content().Column(col =>
                {
                    col.Spacing(6);

                    void AddRow(string label, string value)
                    {
                        col.Item().Row(row =>
                        {
                            row.ConstantItem(160).Background("#F5F7FA").Padding(6)
                                .Text(label).Bold().FontSize(9).FontColor("#1E3A5F");
                            row.RelativeItem().BorderBottom(1).BorderColor("#E0E0E0").Padding(6)
                                .Text(value).FontSize(9);
                        });
                    }

                    AddRow("Request Number", r.RequestNumber);
                    AddRow("Title", r.Title);
                    AddRow("Description", r.Description ?? "-");
                    AddRow("Category", r.CategoryName);
                    AddRow("Priority", r.Priority.ToString());
                    AddRow("Status", r.Status.ToString());
                    AddRow("Employee", r.EmployeeName);
                    AddRow("Assignee", r.AssigneeName ?? "-");
                    AddRow("Created On", r.CreatedOn.ToString("yyyy-MM-dd HH:mm"));
                    AddRow("Updated On", r.UpdatedOn?.ToString("yyyy-MM-dd HH:mm") ?? "-");
                    AddRow("Resolved On", r.ResolvedOn?.ToString("yyyy-MM-dd HH:mm") ?? "-");
                    AddRow("Closed On", r.ClosedOn?.ToString("yyyy-MM-dd HH:mm") ?? "-");
                    AddRow("Rejection Reason", r.RejectionReason ?? "-");
                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Generated on ");
                    x.Span(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm"));
                    x.Span(" UTC");
                });
            });
        }).GeneratePdf();
    }

    // ── Excel stubs — implemented in ExcelExportService ───────────────────────
    public byte[] ExportRequestListToExcel(IReadOnlyList<RequestListDto> requests)
        => throw new NotImplementedException("Use ExcelExportService");

    public byte[] ExportRequestDetailToExcel(RequestDetailDto request)
        => throw new NotImplementedException("Use ExcelExportService");
}