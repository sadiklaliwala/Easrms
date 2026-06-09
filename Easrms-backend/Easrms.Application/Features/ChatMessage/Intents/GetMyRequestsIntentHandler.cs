using Easrms.Application.DTOs.Chat;
using Easrms.Application.DTOs.Request;
using Easrms.Application.Interfaces.Repositories;
using System.Threading.Tasks;

namespace Easrms.Application.Features.ChatMessage.Intents;

public class GetMyRequestsIntentHandler : IIntentHandler
{
    private readonly IRequestRepository _requestRepository;

    public GetMyRequestsIntentHandler(IRequestRepository requestRepository)
    {
        _requestRepository = requestRepository;
    }

    public string IntentName => "get_my_requests";

    public async Task<string> HandleAsync(ChatIntentDto intent, ChatMessageCommand command)
    {
        var queryParams = new RequestQueryParams
        {
            EmployeeId = command.CurrentUserId,
            Status = intent.Filters?.Status,
            Priority = intent.Filters?.Priority,
            PageNumber = 1,
            PageSize = 5
        };
        var result = await _requestRepository.GetPagedRequestsAsync(queryParams);
        return ContextBuilder.BuildRequestListContext(result);
    }
}
