using AutoMapper;
using Easrms.Application.DTOs.Chat;
using Easrms.Application.DTOs.Request;
using Easrms.Application.Interfaces.Repositories;
using System;
using System.Threading.Tasks;

namespace Easrms.Application.Features.ChatMessage.Intents;

public class GetRequestDetailIntentHandler : IIntentHandler
{
    private readonly IRequestRepository _requestRepository;
    private readonly IMapper _mapper;

    public GetRequestDetailIntentHandler(IRequestRepository requestRepository, IMapper mapper)
    {
        _requestRepository = requestRepository;
        _mapper = mapper;
    }

    public string IntentName => "get_request_detail";

    public async Task<string> HandleAsync(ChatIntentDto intent, ChatMessageCommand command)
    {
        if (intent.Filters == null)
            return "Please provide a request number or ID.";

        if (!string.IsNullOrWhiteSpace(intent.Filters.RequestId)
            && Guid.TryParse(intent.Filters.RequestId, out var requestId))
        {
            var detail = await _requestRepository.GetRequestByIdAsync(requestId);
            if (detail == null) return "No request found with that ID.";
            
            if (!RequestAuthorizationHelper.CanViewRequest(detail, command.CurrentUserId, command.CurrentUserRole))
                return "Sorry, you do not have permission to view this request.";

            var mappedDetail = _mapper.Map<RequestDetailDto>(detail);
            return ContextBuilder.BuildRequestDetailContext(mappedDetail);
        }
        else if (!string.IsNullOrWhiteSpace(intent.Filters.RequestNumber))
        {
            var detail = await _requestRepository
                .GetRequestByNumberAsync(intent.Filters.RequestNumber);
            if (detail == null) return $"No request found with number {intent.Filters.RequestNumber}.";

            if (!RequestAuthorizationHelper.CanViewRequest(detail, command.CurrentUserId, command.CurrentUserRole))
                return "Sorry, you do not have permission to view this request.";

            var mappedDetail = _mapper.Map<RequestDetailDto>(detail);
            return ContextBuilder.BuildRequestDetailContext(mappedDetail);
        }
        else
        {
            return "Please provide a request number or ID.";
        }
    }
}
