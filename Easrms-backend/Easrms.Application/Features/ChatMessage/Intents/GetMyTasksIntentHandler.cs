using Easrms.Application.DTOs.Chat;
using Easrms.Application.DTOs.Request;
using Easrms.Application.Interfaces.Repositories;
using System.Threading.Tasks;

namespace Easrms.Application.Features.ChatMessage.Intents;

public class GetMyTasksIntentHandler : IIntentHandler
{
    private readonly IRequestRepository _requestRepository;

    public GetMyTasksIntentHandler(IRequestRepository requestRepository)
    {
        _requestRepository = requestRepository;
    }

    public string IntentName => "get_my_tasks";

    public async Task<string> HandleAsync(ChatIntentDto intent, ChatMessageCommand command)
    {
        var queryParams = new RequestQueryParams
        {
            AssignedTo = command.CurrentUserId,
            Status = intent.Filters?.Status,
            PageNumber = 1,
            PageSize = 5
        };
        var result = await _requestRepository.GetPagedRequestsAsync(queryParams);
        return ContextBuilder.BuildRequestListContext(result);
    }
}
