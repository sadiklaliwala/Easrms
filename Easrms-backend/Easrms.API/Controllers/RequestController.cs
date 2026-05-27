using Easrms.Application.Features.Request.Commands;
using Easrms.Application.Features.Request.Queries;
using Easrms.Application.DTOs.Request;
using Easrms.Common.Constants;
using Easrms.Common.Response;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Easrms.Application.Features.Dashboard.Queries;

namespace Easrms.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RequestController : ControllerBase
{
    private readonly IMediator _mediator;

    public RequestController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // GET /api/requests
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] string? status = null,
        [FromQuery] string? priority = null,
       [FromQuery] Guid? categoryId = null,
    [FromQuery] DateTime? fromDate = null,
    [FromQuery] DateTime? toDate = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool? sortAscending = null,
        CancellationToken cancellationToken = default)
   {
        var currentUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var roleName = User.FindFirstValue(ClaimTypes.Role)!;

        var query = new GetAllRequestsQuery
        {
            CurrentUserId = currentUserId,
            CurrentUserRole = roleName,
            PageNumber = pageNumber,
            PageSize = pageSize,
            Search = search,
            Status = status,
            Priority = priority,
            CategoryId = categoryId,
            FromDate = fromDate,
            ToDate = toDate,
            SortBy = sortBy,
            SortAscending = sortAscending
        };

        var result = await _mediator.Send(query, cancellationToken);

        return Ok(new ApiResponse<object>
        {
            Success = true,
            StatusCode = 200,
            Message = "Requests retrieved successfully.",
            Data = result,
            Errors = null
        });
    }

    // GET /api/requests/{id}
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var query = new GetRequestByIdQuery { RequestId = id };

        var result = await _mediator.Send(query, cancellationToken);

        return Ok(new ApiResponse<object>
        {
            Success = true,
            StatusCode = 200,
            Message = "Request retrieved successfully.",
            Data = result,
            Errors = null
        });
    }

    // POST /api/requests
    [HttpPost]
    //[Authorize(Roles = RoleConstants.Employee)]
    [Authorize]

    public async Task<IActionResult> Create(
        [FromBody] CreateRequestDto dto,
        CancellationToken cancellationToken = default)
    {
        var currentUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var command = new CreateRequestCommand
        {
            CategoryId = dto.CategoryId,
            Title = dto.Title,
            Description = dto.Description,
            Priority = dto.Priority,
            CurrentUserId = currentUserId,
            AttachmentUrl = dto.AttachmentUrl
        };

        var requestNumber = await _mediator.Send(command, cancellationToken);

        return StatusCode(201, new ApiResponse<object>
        {
            Success = true,
            StatusCode = 201,
            Message = "Request created successfully.",
            Data = new { RequestNumber = requestNumber },
            Errors = null
        });
    }

    // POST /api/requests/{id}/approval
    [HttpPost("{id:guid}/approval")]
    [Authorize(Roles = RoleConstants.Manager)]
    public async Task<IActionResult> Approval(
        Guid id,
        [FromBody] ApprovalRequestDto dto,
        CancellationToken cancellationToken = default)
    {
        var currentUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        
        var command = new ApprovalRequestCommand
        {
            RequestId = id,
            Action = dto.Action,
            Comment = dto.Comment,
            CurrentUserId = currentUserId
        };

        await _mediator.Send(command, cancellationToken);

        return Ok(new ApiResponse<object>
        {
            Success = true,
            StatusCode = 200,
            Message = "Request approval action completed successfully.",
            Data = null,
            Errors = null
        });
    }

    // POST /api/requests/{id}/assign
    [HttpPost("{id:guid}/assign")]
    [Authorize(Roles = RoleConstants.Admin)]
    public async Task<IActionResult> Assign(
        Guid id,
        [FromBody] AssignRequestDto dto,
        CancellationToken cancellationToken = default)
    {

        var currentUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var command = new AssignRequestCommand
        {
            RequestId = id,
            SupportUserId = dto.SupportUserId,
            CurrentUserId= currentUserId
        };

        await _mediator.Send(command, cancellationToken);

        return Ok(new ApiResponse<object>
        {
            Success = true,
            StatusCode = 200,
            Message = "Request assigned successfully.",
            Data = null,
            Errors = null
        });
    }

    // POST /api/requests/{id}/status
    [HttpPost("{id:guid}/status")]
    [Authorize(Roles = RoleConstants.SupportUser)]
    public async Task<IActionResult> UpdateStatus(
        Guid id,
        [FromBody] UpdateStatusDto dto,
        CancellationToken cancellationToken = default)
    {
        var currentUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var command = new UpdateRequestStatusCommand
        {
            RequestId = id,
            NewStatus = dto.NewStatus,
            Remarks = dto.Remarks,
            CurrentUserId = currentUserId
        };

        await _mediator.Send(command, cancellationToken);

        return Ok(new ApiResponse<object>
        {
            Success = true,
            StatusCode = 200,
            Message = "Request status updated successfully.",
            Data = null,
            Errors = null
        });
    }

    // PUT /api/requests/{id}/close
    [HttpPut("{id:guid}/close")]
    public async Task<IActionResult> Close(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var currentUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var roleName = User.FindFirstValue(ClaimTypes.Role)!;

        var command = new CloseRequestCommand
        {
            RequestId = id,
            CurrentUserId = currentUserId,
            CurrentUserRole = roleName
        };

        await _mediator.Send(command, cancellationToken);

        return Ok(new ApiResponse<object>
        {
            Success = true,
            StatusCode = 200,
            Message = "Request closed successfully.",
            Data = null,
            Errors = null
        });
    }

    // POST /api/requests/{id}/escalate
    [HttpPost("{id:guid}/escalate")]
    [Authorize(Roles = RoleConstants.Admin)]
    public async Task<IActionResult> Escalate(
        Guid id,
        [FromBody] EscalateRequestDto dto,
        CancellationToken cancellationToken = default)
    {
        var currentUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var command = new EscalateRequestCommand
        {
            RequestId = id,
            CurrentUserId = currentUserId,
            Dto = dto
        };

        await _mediator.Send(command, cancellationToken);

        return Ok(new ApiResponse<object>
        {
            Success = true,
            StatusCode = 200,
            Message = "Request escalated successfully.",
            Data = null,
            Errors = null
        });
    }

    // GET /dashboard/sla-summary
    [HttpGet("../dashboard/sla-summary")]
    [Authorize(Roles = RoleConstants.Admin + "," + RoleConstants.Manager)]
    public async Task<IActionResult> GetSLASummary(CancellationToken cancellationToken = default)
    {
        var currentUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var roleName = User.FindFirstValue(ClaimTypes.Role)!;

        var query = new GetSLADashboardQuery
        {
            CurrentUserId = currentUserId,
            CurrentUserRole = roleName
        };

        var result = await _mediator.Send(query, cancellationToken);

        return Ok(new ApiResponse<object>
        {
            Success = true,
            StatusCode = 200,
            Message = "SLA summary retrieved successfully.",
            Data = result,
            Errors = null
        });
    }

    // POST /api/requests/bulk
    [HttpPost("bulk")]
    [Authorize(Roles = "Admin,Employee")]
    [RequestSizeLimit(5_242_880)]
    public async Task<IActionResult> BulkUpload(IFormFile file, CancellationToken cancellationToken = default)
    {
        if (file == null) return BadRequest(ApiResponse<object>.FailResponse("No file provided", 400));

        var currentUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        using var ms = new MemoryStream();
        await file.CopyToAsync(ms, cancellationToken);

        var command = new Easrms.Application.Features.Request.Commands.BulkCreateRequests.BulkCreateRequestsCommand
        {
            FileName = file.FileName,
            FileContent = ms.ToArray(),
            EmployeeId = currentUserId
        };

        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }
}