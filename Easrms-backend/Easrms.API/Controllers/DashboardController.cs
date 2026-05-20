using Easrms.Application.Features.Dashboard.Queries;
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
public class DashboardController : ControllerBase
{
    private readonly IMediator _mediator;

    public DashboardController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // GET /api/dashboard/summary
    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary(
        CancellationToken cancellationToken = default)
    {
        var currentUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var roleName = User.FindFirstValue(ClaimTypes.Role)!;

        var query = new GetDashboardSummaryQuery
        {
            CurrentUserId = currentUserId,
            RoleName = roleName
        };

        var result = await _mediator.Send(query, cancellationToken);

        return Ok(new ApiResponse<object>
        {
            Success = true, 
            StatusCode = 200,
            Message = "Dashboard summary retrieved successfully.",
            Data = result,
            Errors = null
        });
    }

    // GET /api/dashboard/sla-summary
    [HttpGet("sla-summary")]
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
}