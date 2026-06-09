using Easrms.Application.DTOs.Chat;
using Easrms.Common.Response;
using Easrms.Infrastructure.Services;
using MediatR;
using System.Text.Json;
using Easrms.Application.Features.ChatMessage.Intents;

namespace Easrms.Application.Features.ChatMessage;

public class ChatMessageCommandHandler
    : IRequestHandler<ChatMessageCommand, ApiResponse<ChatMessageResponseDto>>
{
    private readonly IClaudeService _claudeService;
    private readonly IEnumerable<IIntentHandler> _intentHandlers;

    public ChatMessageCommandHandler(
        IClaudeService claudeService,
        IEnumerable<IIntentHandler> intentHandlers
        )
    {
        _claudeService = claudeService;
        _intentHandlers = intentHandlers;
    }

    public async Task<ApiResponse<ChatMessageResponseDto>> Handle(
        ChatMessageCommand command,
        CancellationToken cancellationToken)
    {
        ChatIntentDto intent;
        try
        {
            // ── Step 1: Ask AI to parse the user's intent ──────────────────────
            var intentJson = await _claudeService.ResolveIntentAsync(
                command.Message,
                command.CurrentUserRole);

            // Safety check for invalid AI responses
            if (string.IsNullOrWhiteSpace(intentJson) ||
                !intentJson.TrimStart().StartsWith("{"))
            {
                intentJson = """
        {
          "intent": "unknown",
          "filters": {
            "status": null,
            "priority": null,
            "requestNumber": null,
            "requestId": null
          }
        }
        """;
            }

            intent = JsonSerializer.Deserialize<ChatIntentDto>(
                intentJson,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                })
                ?? new ChatIntentDto
                {
                    Intent = "unknown"
                };

            // Fix AI mistakes: if RequestId is not a GUID, it's probably the RequestNumber
            if (intent.Filters != null &&
                !string.IsNullOrWhiteSpace(intent.Filters.RequestId) &&
                !Guid.TryParse(intent.Filters.RequestId, out _))
            {
                if (string.IsNullOrWhiteSpace(intent.Filters.RequestNumber))
                {
                    intent.Filters.RequestNumber = intent.Filters.RequestId;
                }
                intent.Filters.RequestId = null;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);

            return ApiResponse<ChatMessageResponseDto>.SuccessResponse(
                new ChatMessageResponseDto
                {
                    Reply = "I am currently experiencing technical difficulties processing your request. Please try again later.",
                    Intent = "error"
                },
                "Failed to reach AI service."
            );
        }

        // ── Step 2: Route to the correct handler based on intent ────────────
        string dataContext = string.Empty;

        var handler = _intentHandlers.FirstOrDefault(h => h.IntentName == intent.Intent);
        
        if (handler != null)
        {
            dataContext = await handler.HandleAsync(intent, command);
        }

        // ── Step 3: Ask Claude to format a friendly reply using the fetched data ─
        var friendlyReply = await _claudeService.FormatReplyAsync(
            command.Message,
            intent.Intent,
            dataContext,
            command.CurrentUserName,
            command.CurrentUserRole);

        return ApiResponse<ChatMessageResponseDto>.SuccessResponse(
            new ChatMessageResponseDto
            {
                Reply = friendlyReply,
                Intent = intent.Intent
            },
            "Chat response generated.");
    }
}
