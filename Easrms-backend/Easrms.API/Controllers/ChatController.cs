using Easrms.Application.DTOs.Chat;
using Easrms.Application.Features.ChatMessage;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Easrms.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // All roles can use chat — role scoping is handled inside the handler
public class ChatController : ControllerBase
{
    private readonly IMediator _mediator;

    public ChatController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // POST /api/chat/message
    [HttpPost("message")]
    public async Task<IActionResult> SendMessage([FromBody] ChatMessageRequestDto dto)
    {
        var command = new ChatMessageCommand
        {
            Message         = dto.Message,
            // Pull user info from JWT claims — same pattern used in all other controllers
            CurrentUserId   = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!),
            CurrentUserRole = User.FindFirstValue(ClaimTypes.Role)!,
            CurrentUserName = User.FindFirstValue(ClaimTypes.Name)!
        };

        var result = await _mediator.Send(command);
        return StatusCode(result.StatusCode, result);
    }
}
