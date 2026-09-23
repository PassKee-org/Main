using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.Instances;

namespace PassKee.Orm.Core.Conventions;

public class TableNameConvention : IClassConvention
{
    public void Apply(IClassInstance instance)
    {
        var snakeCasedTableName = instance.EntityType.Name
            .Replace("Entity", "");
        instance.Table(snakeCasedTableName);
    }
}
