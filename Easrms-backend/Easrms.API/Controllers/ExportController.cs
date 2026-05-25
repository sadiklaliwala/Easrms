using DocumentFormat.OpenXml.Wordprocessing;
using Easrms.Application.DTOs.Request;
using Easrms.Application.Features.Request.Queries;
using Easrms.Application.Interfaces;
using Easrms.Common.Constants;
using Easrms.Infrastructure.Export;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.NetworkInformation;
using System.Security.Claims;
using System.Threading;

namespace Easrms.API.Controllers;

[ApiController]
[Route("api/export")]
[Authorize(Roles = RoleConstants.Admin)]
public class ExportController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ExcelExportService _excel;
    private readonly PdfExportService _pdf;

    public ExportController(
        IMediator mediator,
        ExcelExportService excel,
        PdfExportService pdf)
    {
        _mediator = mediator;
        _excel = excel;
        _pdf = pdf;
    }

    // ── Request List Exports ──────────────────────────────────────────────────

    [HttpGet("requests/excel")]
    public async Task<IActionResult> ExportRequestListExcel([FromQuery] RequestQueryParams queryParams ,CancellationToken cancellationToken = default)
    {
        var currentUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var roleName = User.FindFirstValue(ClaimTypes.Role)!;

        var query = new GetAllRequestsQuery
        {
            CurrentUserId = currentUserId,
            CurrentUserRole = roleName,
            PageNumber = queryParams.PageNumber,
            PageSize = queryParams.PageSize,
            Search = queryParams.SearchTerm,
            Status = queryParams.Status,
            Priority = queryParams.Priority,
            CategoryId = queryParams.CategoryId,
            FromDate = queryParams.FromDate,
            ToDate = queryParams.ToDate
        };
        var result = await _mediator.Send(query, cancellationToken);
        var bytes = _excel.ExportRequestListToExcel(result.Items);
        return File(bytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"Requests_{DateTime.UtcNow:yyyyMMdd_HHmm}.xlsx");
    }

    [HttpGet("requests/pdf")]
    public async Task<IActionResult> ExportRequestListPdf([FromQuery] RequestQueryParams queryParams,CancellationToken cancellationToken = default )
    {
        var currentUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var roleName = User.FindFirstValue(ClaimTypes.Role)!;

        var query = new GetAllRequestsQuery
        {
            CurrentUserId = currentUserId,
            CurrentUserRole = roleName,
            PageNumber = queryParams.PageNumber,
            PageSize = queryParams.PageSize,
            Search = queryParams.SearchTerm,
            Status = queryParams.Status,
            Priority = queryParams.Priority,
            CategoryId = queryParams.CategoryId,
            FromDate = queryParams.FromDate,
            ToDate = queryParams.ToDate
        };
        var result = await _mediator.Send(query, cancellationToken);
        var bytes = _pdf.ExportRequestListToPdf(result.Items);
        return File(bytes, "application/pdf",
            $"Requests_{DateTime.UtcNow:yyyyMMdd_HHmm}.pdf");
    }

    // ── Request Detail Exports ────────────────────────────────────────────────

    [HttpGet("requests/{id:guid}/excel")]
    public async Task<IActionResult> ExportRequestDetailExcel(Guid id,CancellationToken cancellationToken = default)
    {
        var query = new GetRequestByIdQuery { RequestId = id };

        var result = await _mediator.Send(query, cancellationToken);
        if (result is null) return NotFound();
        var bytes = _excel.ExportRequestDetailToExcel(result);
        return File(bytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"Request_{result.RequestNumber}.xlsx");
    }

    [HttpGet("requests/{id:guid}/pdf")]
    public async Task<IActionResult> ExportRequestDetailPdf(Guid id, CancellationToken cancellationToken = default)
    {
        var query = new GetRequestByIdQuery { RequestId = id };

        var result = await _mediator.Send(query, cancellationToken);
        if (result is null) return NotFound();
        var bytes = _pdf.ExportRequestDetailToPdf(result);
        return File(bytes, "application/pdf",
            $"Request_{result.RequestNumber}.pdf");
    }
}