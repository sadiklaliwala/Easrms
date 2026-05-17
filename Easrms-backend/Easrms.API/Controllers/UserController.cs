using Easrms.Application.Features.User.Commands;
using Easrms.Application.Features.User.Queries;
using Easrms.Application.DTOs.User;
using Easrms.Common.Constants;
using Easrms.Common.Response;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Easrms.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = RoleConstants.Admin)]
public class UserController : ControllerBase
{
    private readonly IMediator _mediator;

    public UserController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // GET /api/users
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] Guid? roleId = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortDirection = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAllUsersQuery(new UserQueryParams
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            Search = search,
            RoleId = roleId,
            IsActive = isActive,
            SortBy = sortBy,
            SortDirection = sortDirection
        });

        var result = await _mediator.Send(query, cancellationToken);

        return Ok(new ApiResponse<object>
        {
            Success = true,
            StatusCode = 200,
            Message = "Users retrieved successfully.",
            Data = result,
            Errors = null
        });
    }

    // GET /api/users/{id}
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var query = new GetUserByIdQuery(id);
        var result = await _mediator.Send(query, cancellationToken);

        return Ok(new ApiResponse<object>
        {
            Success = true,
            StatusCode = 200,
            Message = "User retrieved successfully.",
            Data = result,
            Errors = null
        });
    }

    // POST /api/users
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateUserDto dto,
        CancellationToken cancellationToken = default)
    {
        var command = new CreateUserCommand(dto);
        var newId = await _mediator.Send(command, cancellationToken);

        return StatusCode(201, new ApiResponse<object>
        {
            Success = true,
            StatusCode = 201,
            Message = "User created successfully.",
            Data = new { UserId = newId },
            Errors = null
        });
    }

    // PUT /api/users/{id}
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateUserDto dto,
        CancellationToken cancellationToken = default)
    {
        var command = new UpdateUserCommand(id, dto);
        await _mediator.Send(command, cancellationToken);

        return Ok(new ApiResponse<object>
        {
            Success = true,
            StatusCode = 200,
            Message = "User updated successfully.",
            Data = null,
            Errors = null
        });
    }

    // PUT /api/users/{id}/activate-deactivate
    [HttpPut("{id:guid}/activate-deactivate")]
    public async Task<IActionResult> ToggleStatus(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var command = new ToggleUserStatusCommand(id);
        await _mediator.Send(command, cancellationToken);

        return Ok(new ApiResponse<object>
        {
            Success = true,
            StatusCode = 200,
            Message = "User status updated successfully.",
            Data = null,
            Errors = null
        });
    }
}