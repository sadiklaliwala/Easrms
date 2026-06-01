using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace Easrms.Common.Hubs;

public class NotificationHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var connectionId = Context.ConnectionId;

        var role = Context.User?.FindFirst(ClaimTypes.Role)?.Value;
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!string.IsNullOrWhiteSpace(role))
        {
            await Groups.AddToGroupAsync(connectionId, role);
        }

        if (!string.IsNullOrWhiteSpace(userId))
        {
            await Groups.AddToGroupAsync(connectionId, $"user_{userId}");
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var connectionId = Context.ConnectionId;

        var role = Context.User?.FindFirst(ClaimTypes.Role)?.Value;
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!string.IsNullOrWhiteSpace(role))
        {
            await Groups.RemoveFromGroupAsync(connectionId, role);
        }

        if (!string.IsNullOrWhiteSpace(userId))
        {
            await Groups.RemoveFromGroupAsync(connectionId, $"user_{userId}");
        }

        await base.OnDisconnectedAsync(exception);
    }
}
