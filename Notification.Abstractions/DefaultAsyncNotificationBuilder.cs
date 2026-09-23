using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Notification.Abstractions
{
    public class DefaultAsyncNotificationBuilder: IAsyncNotificationBuilder
    {
        private readonly IAsyncNotificationFactory _asyncNotificationFactory;

        public DefaultAsyncNotificationBuilder(IAsyncNotificationFactory asyncCommandFactory)
        {
            _asyncNotificationFactory = asyncCommandFactory ?? throw new ArgumentNullException(nameof(asyncCommandFactory));
        }

        public Task SendAsync<INotificationContext>(
            INotificationContext commandContext,
            CancellationToken cancellationToken = default
        )
            where INotificationContext : Abstractions.INotificationItemContext
        {
            return _asyncNotificationFactory.Create<INotificationContext>().SendAsync(commandContext, cancellationToken);
        }
    }
}
