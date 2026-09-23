using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Notification.Abstractions
{
    public interface IAsyncNotificationBuilder
    {
        Task SendAsync<INotificationContext>(INotificationContext commandContext, CancellationToken cancellationToken = default) 
            where INotificationContext : Abstractions.INotificationItemContext;
    }
}
