using Easrms.Application.DTOs.Chat;
using Easrms.Common.Response;
using MediatR;

namespace Easrms.Application.Features.ChatMessage;

public class ChatMessageCommand : IRequest<ApiResponse<ChatMessageResponseDto>>
{
    public string Message { get; set; } = string.Empty;

    // Populated by the controller from JWT claims — same pattern as all other commands
    public Guid CurrentUserId { get; set; }
    public string CurrentUserRole { get; set; } = string.Empty;
    public string CurrentUserName { get; set; } = string.Empty;
}
