using Easrms.Application.Features.Lookup.Queries;
using Easrms.Common.Response;
using Easrms.Common.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Easrms.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = RoleConstants.Admin)]
public class LookupController : ControllerBase
{
    private readonly IMediator _mediator;

    public LookupController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // GET /api/lookup/support-users
    [HttpGet("support-users")]
    public async Task<IActionResult> GetSupportUsers(
        CancellationToken cancellationToken = default)
    {
        var query = new GetSupportUsersQuery();
        var result = await _mediator.Send(query, cancellationToken);

        return Ok(new ApiResponse<object>
        {
            Success = true,
            StatusCode = 200,
            Message = "Support users retrieved successfully.",
            Data = result,
            Errors = null
        });
    }

    // GET /api/lookup/managers
    [HttpGet("managers")]
    public async Task<IActionResult> GetManagers(
        CancellationToken cancellationToken = default)
    {
        var query = new GetManagersQuery();
        var result = await _mediator.Send(query, cancellationToken);

        return Ok(new ApiResponse<object>
        {
            Success = true,
            StatusCode = 200,
            Message = "Managers retrieved successfully.",
            Data = result,
            Errors = null
        });
    }
}