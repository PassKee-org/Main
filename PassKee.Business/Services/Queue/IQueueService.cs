using Domain.Abstractions;
using Notification.Abstractions;
using PassKee.Orm.Constants;

namespace PassKee.Business.Services.Queue;

public interface IQueueService: IDomainService
{
    Task PushNotificationAsync(INotificationItemContext itemContext);

    Task PushDefaultAsync(IQueueItemContext itemContext);

    Task PushExternalClientAsync(IExternalServiceItemContext itemContext);

    Task<int> ProcessAsync(
        QueueChannel channel,
        CancellationToken cancellationToken = default,
        bool isClearSessionForEachIteration = true
    );
}
