using Easrms.Application.DTOs.Chat;
using Easrms.Application.DTOs.Request;
using Easrms.Application.Interfaces.Repositories;
using Easrms.Common.Constants;
using System.Threading.Tasks;

namespace Easrms.Application.Features.ChatMessage.Intents;

public class GetMyApprovalsIntentHandler : IIntentHandler
{
    private readonly IRequestRepository _requestRepository;

    public GetMyApprovalsIntentHandler(IRequestRepository requestRepository)
    {
        _requestRepository = requestRepository;
    }

    public string IntentName => "get_my_approvals";

    public async Task<string> HandleAsync(ChatIntentDto intent, ChatMessageCommand command)
    {
        var queryParams = new RequestQueryParams
        {
            Status = StatusConstants.PendingApproval,
            ManagerId = command.CurrentUserId,
            PageNumber = 1,
            PageSize = 5
        };
        var result = await _requestRepository.GetPagedRequestsAsync(queryParams);
        return ContextBuilder.BuildRequestListContext(result);
    }
}
