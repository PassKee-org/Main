using NHibernate.Event;

namespace PassKee.Orm.Core.Interceptors.Abstract;

public interface IEntityPreFlushEvent
{
    public void OnInsert(PreInsertEvent @event);
    
    public void OnFlush(FlushEntityEvent @event);
}
