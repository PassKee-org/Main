using System;
using Autofac;

namespace Notification.Abstractions
{
    public class ScopedAsyncNotificationFactory : IAsyncNotificationFactory
    {
        private readonly ILifetimeScope _scope;

        public ScopedAsyncNotificationFactory(ILifetimeScope scope)
        {
            _scope = scope ?? throw new ArgumentNullException(nameof(scope));
        }

        public IAsyncNotification<TNotificationContext> Create<TNotificationContext>() where TNotificationContext : INotificationItemContext
        {
            return _scope.Resolve<IAsyncNotification<TNotificationContext>>();
        }
    }
}