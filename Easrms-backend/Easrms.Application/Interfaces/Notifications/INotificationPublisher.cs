using System.Threading;

namespace Easrms.Application.Interfaces.Notifications;

public interface INotificationPublisher
{
    Task PublishToGroupAsync(string groupName, string eventName, object payload, CancellationToken cancellationToken = default);
    Task PublishToUserAsync(Guid userId, string eventName, object payload, CancellationToken cancellationToken = default);
}
