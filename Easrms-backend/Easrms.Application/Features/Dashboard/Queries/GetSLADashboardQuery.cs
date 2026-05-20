using Easrms.Application.DTOs.Dashboard;
using Easrms.Application.Interfaces.Repositories;
using MediatR;

namespace Easrms.Application.Features.Dashboard.Queries;

public record GetSLADashboardQuery : IRequest<SLADashboardDto>
{
    public Guid CurrentUserId { get; init; }
    public string CurrentUserRole { get; init; } = string.Empty;
}

public class GetSLADashboardQueryHandler : IRequestHandler<GetSLADashboardQuery, SLADashboardDto>
{
    private readonly IDashboardRepository _dashboardRepository;

    public GetSLADashboardQueryHandler(IDashboardRepository dashboardRepository)
    {
        _dashboardRepository = dashboardRepository;
    }

    public async Task<SLADashboardDto> Handle(GetSLADashboardQuery request, CancellationToken cancellationToken)
    {
        var queryParams = new DashboardQueryParams();

        switch (request.CurrentUserRole)
        {
            case "Employee":
                queryParams.EmployeeId = request.CurrentUserId;
                break;
            case "Manager":
                queryParams.ManagerId = request.CurrentUserId;
                break;
            case "SupportUser":
                queryParams.AssignedToUserId = request.CurrentUserId;
                break;
            case "Admin":
            default:
                break;
        }

        var summary = await _dashboardRepository.GetSLASummaryAsync(queryParams, cancellationToken);

        return new SLADashboardDto
        {
            TotalOpen = summary.OpenCount,
            WithinSLACount = summary.WithinSLACount,
            NearingBreachCount = summary.NearingBreachCount,
            BreachedCount = summary.BreachedCount,
            EscalatedCount = summary.EscalatedCount
        };
    }
}
