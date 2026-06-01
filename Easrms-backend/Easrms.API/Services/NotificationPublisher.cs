using Easrms.Application.Interfaces.Notifications;
using Easrms.Common.Constants;
using Easrms.Common.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace Easrms.API.Services;

public class NotificationPublisher : INotificationPublisher
{
    private readonly IHubContext<Easrms.API.Hubs.NotificationHub> _hubContext;

    public NotificationPublisher(IHubContext<Easrms.API.Hubs.NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public Task PublishToGroupAsync(string groupName, string eventName, object payload, CancellationToken cancellationToken = default)
    {
        return _hubContext.Clients.Group(groupName).SendAsync(eventName, payload, cancellationToken);
    }

    public Task PublishToUserAsync(Guid userId, string eventName, object payload, CancellationToken cancellationToken = default)
    {
        return _hubContext.Clients.Group($"user_{userId}").SendAsync(eventName, payload, cancellationToken);
    }
}
