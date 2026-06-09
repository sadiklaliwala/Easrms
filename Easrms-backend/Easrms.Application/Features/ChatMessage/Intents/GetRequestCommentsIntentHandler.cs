using Easrms.Application.DTOs.Chat;
using Easrms.Application.Interfaces.Repositories;
using System;
using System.Threading.Tasks;

namespace Easrms.Application.Features.ChatMessage.Intents;

public class GetRequestCommentsIntentHandler : IIntentHandler
{
    private readonly IRequestRepository _requestRepository;
    private readonly ICommentRepository _commentRepository;

    public GetRequestCommentsIntentHandler(IRequestRepository requestRepository, ICommentRepository commentRepository)
    {
        _requestRepository = requestRepository;
        _commentRepository = commentRepository;
    }

    public string IntentName => "get_request_comments";

    public async Task<string> HandleAsync(ChatIntentDto intent, ChatMessageCommand command)
    {
        Guid? targetId = null;
        if (intent.Filters != null)
        {
            if (!string.IsNullOrWhiteSpace(intent.Filters.RequestId) && Guid.TryParse(intent.Filters.RequestId, out var parsedId))
                targetId = parsedId;
            else if (!string.IsNullOrWhiteSpace(intent.Filters.RequestNumber))
            {
                var req = await _requestRepository.GetRequestByNumberAsync(intent.Filters.RequestNumber);
                if (req != null) targetId = req.RequestId;
            }
        }

        if (targetId.HasValue)
        {
            var request = await _requestRepository.GetRequestByIdAsync(targetId.Value);
            if (request == null)
            {
                return "No request found with that ID or Number.";
            }

            if (!RequestAuthorizationHelper.CanViewRequest(request, command.CurrentUserId, command.CurrentUserRole))
                return "Sorry, you do not have permission to view comments for this request.";

            var comments = await _commentRepository
                .GetCommentsByRequestIdAsync(targetId.Value);
            return ContextBuilder.BuildCommentsContext(comments);
        }
        else
        {
            return "Please provide a valid request number (e.g., REQ-0001) to fetch its comments.";
        }
    }
}
