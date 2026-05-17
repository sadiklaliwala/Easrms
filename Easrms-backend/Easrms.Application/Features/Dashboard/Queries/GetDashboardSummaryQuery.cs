using Easrms.Application.DTOs.Dashboard;
using Easrms.Application.Interfaces.Repositories;
using Easrms.Common.Constants;
using MediatR;

namespace Easrms.Application.Features.Dashboard.Queries;

/// <summary>
/// Query to retrieve the role-scoped dashboard summary.
/// The handler resolves the caller's role from JWT claims and builds
/// the appropriate <see cref="DashboardQueryParams"/> before delegating
/// to <see cref="IDashboardRepository.GetSummaryAsync"/>.
/// </summary>
/// <param name="CurrentUserId">The authenticated user's ID, extracted from JWT claims.</param>
/// <param name="RoleName">The authenticated user's role name, extracted from JWT claims.</param>
public record GetDashboardSummaryQuery : IRequest<DashboardSummaryDto>
{
    public Guid CurrentUserId { get; init; }
    public string RoleName { get; init; } = string.Empty;
}


/// <summary>
/// Handles <see cref="GetDashboardSummaryQuery"/>.
///
/// Flow:
///   1. Resolve caller's role and build <see cref="DashboardQueryParams"/> with the
///      appropriate scope filter (EmployeeId, ManagerId, AssignedToUserId, or global).
///   2. Delegate to <see cref="IDashboardRepository.GetSummaryAsync"/> — all aggregation
///      logic lives in the repository (Dapper).
///   3. Return the populated <see cref="DashboardSummaryDto"/> directly.
///
/// No 404/403 guards needed here — the caller is always the authenticated user
/// and the scope filter simply restricts the data they see.
/// </summary>
public class GetDashboardSummaryQueryHandler
    : IRequestHandler<GetDashboardSummaryQuery, DashboardSummaryDto>
{
    private readonly IDashboardRepository _dashboardRepository;

    public GetDashboardSummaryQueryHandler(IDashboardRepository dashboardRepository)
    {
        _dashboardRepository = dashboardRepository;
    }

    public async Task<DashboardSummaryDto> Handle(
        GetDashboardSummaryQuery request,
        CancellationToken cancellationToken)
    {
        // Build role-scoped query params.
        // The repository stays role-agnostic — it applies whatever scope filters
        // are set in the params object without knowing why they were set.
        var queryParams = new DashboardQueryParams();

        switch (request.RoleName)
        {
            case RoleConstants.Employee:
                queryParams.EmployeeId = request.CurrentUserId;
                break;

            case RoleConstants.Manager:
                queryParams.ManagerId = request.CurrentUserId;
                break;

            case RoleConstants.SupportUser:
                queryParams.AssignedToUserId = request.CurrentUserId;
                break;

            case RoleConstants.Admin:
            default:
                // All nulls → global scope, no WHERE clause filter applied
                break;
        }

        return await _dashboardRepository.GetSummaryAsync(queryParams, cancellationToken);
    }
}