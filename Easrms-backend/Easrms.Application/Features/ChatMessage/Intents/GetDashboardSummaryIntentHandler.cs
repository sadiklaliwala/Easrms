using Easrms.Application.DTOs.Chat;
using Easrms.Application.DTOs.Dashboard;
using Easrms.Application.Interfaces.Repositories;
using Easrms.Common.Constants;
using System.Threading.Tasks;

namespace Easrms.Application.Features.ChatMessage.Intents;

public class GetDashboardSummaryIntentHandler : IIntentHandler
{
    private readonly IDashboardRepository _dashboardRepository;

    public GetDashboardSummaryIntentHandler(IDashboardRepository dashboardRepository)
    {
        _dashboardRepository = dashboardRepository;
    }

    public string IntentName => "get_dashboard_summary";

    public async Task<string> HandleAsync(ChatIntentDto intent, ChatMessageCommand command)
    {
        var queryParams = new DashboardQueryParams();

        if (command.CurrentUserRole == RoleConstants.Manager)
            queryParams.ManagerId = command.CurrentUserId;
        else if (command.CurrentUserRole == RoleConstants.Employee)
            queryParams.EmployeeId = command.CurrentUserId;
        else if (command.CurrentUserRole == RoleConstants.SupportUser)
            queryParams.AssignedToUserId = command.CurrentUserId;
        // Admin → all nulls (global scope)

        var summary = await _dashboardRepository.GetSummaryAsync(queryParams);
        return ContextBuilder.BuildDashboardContext(summary);
    }
}
