using Autofac;
using Microsoft.Extensions.Logging;
using NHibernate.Event;
using PassKee.Orm.Core.Interceptors.Abstract;

namespace PassKee.Orm.Core.Interceptors;

public class EntityPreInsertEventInterceptor : IPreInsertEventListener
{
    private readonly ILogger<EntityPreInsertEventInterceptor> _logger;

    public EntityPreInsertEventInterceptor(ILifetimeScope scope)
    {
        _logger = scope.Resolve<ILogger<EntityPreInsertEventInterceptor>>();
    }

    public Task<bool> OnPreInsertAsync(PreInsertEvent @event, CancellationToken cancellationToken)
    {
        if (@event.Entity is IEntityPreFlushEvent eventEntity)
            eventEntity.OnInsert(@event);
        return Task.FromResult(false);
    }

    public bool OnPreInsert(PreInsertEvent @event)
    {
        return false;
    }
}
