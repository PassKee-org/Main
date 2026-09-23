using Autofac;
using Microsoft.Extensions.Logging;
using NHibernate.Event;
using NHibernate.Event.Default;
using PassKee.Orm.Core.Interceptors.Abstract;

namespace PassKee.Orm.Core.Interceptors;

public class CustomFlushEntityEventListener: DefaultFlushEntityEventListener
{
    private readonly ILogger<CustomFlushEntityEventListener> _logger;

    public CustomFlushEntityEventListener(ILifetimeScope scope) : base()
    {
        _logger = scope.Resolve<ILogger<CustomFlushEntityEventListener>>();
    }

    public override async Task OnFlushEntityAsync(FlushEntityEvent @event, CancellationToken cancellationToken)
    {
        if (@event.Entity is IEntityPreFlushEvent eventEntity)
        {
            if (@event.EntityEntry.ExistsInDatabase)
            {
                // Ignore Inserts
                eventEntity.OnFlush(@event);
            }
        }
        await base.OnFlushEntityAsync(@event, cancellationToken);
    }
    
    public override void OnFlushEntity(FlushEntityEvent @event)
    {
        if (@event.Entity is IEntityPreFlushEvent eventEntity)
        {
            if (@event.EntityEntry.ExistsInDatabase)
            {
                // Ignore Inserts
                eventEntity.OnFlush(@event);
            }
        }

        base.OnFlushEntity(@event);
    }
}
