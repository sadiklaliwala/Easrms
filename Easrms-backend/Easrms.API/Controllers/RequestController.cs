using Easrms.Application.Features.Request.Commands;
using Easrms.Application.Features.Request.Queries;
using Easrms.Application.DTOs.Request;
using Easrms.Common.Constants;
using Easrms.Common.Response;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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
            CategoryId = categoryId
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
            CurrentUserId = currentUserId
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
}