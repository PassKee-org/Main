using System;

namespace Notification.Abstractions
{
    public interface IAsyncNotificationFactory
    {
        IAsyncNotification<TNotificationContext> Create<TNotificationContext>() where TNotificationContext : INotificationItemContext;
    }
}