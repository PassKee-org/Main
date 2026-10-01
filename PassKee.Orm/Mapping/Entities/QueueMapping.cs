using PassKee.Orm.Constants;
using PassKee.Orm.Entities;
using PassKee.Orm.Extensions;
using PassKee.Orm.Mapping.Common;

namespace PassKee.Orm.Mapping.Entities;

public class QueueMapping: BaseGuidMappings<QueueEntity>
{
    public QueueMapping()
    {
        Table("queues");
        
        Map(x => x.Status).Enum<QueueStatus>();
        Map(x => x.Channel).Enum<QueueChannel>();
        Map(x => x.Priority).Enum<QueuePriority>();
        Map(x => x.Error);
        Map(x => x.ContextType);
        Map(x => x.ContextData);
        
        Map(x => x.ProcessAt).DateTime();
    }
}
