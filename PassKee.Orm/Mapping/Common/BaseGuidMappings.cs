using FluentNHibernate.Mapping;
using PassKee.Orm.Core;
using PassKee.Orm.Core.Generators;
using PassKee.Orm.Extensions;

namespace PassKee.Orm.Mapping.Common;

public class BaseGuidMappings<T>: ClassMap<T> where T : AEntity
{
    public BaseGuidMappings()
    {
        Id(x => x.Id)
            .GeneratedBy.Custom<GuidV7Generator>()
            .Unique()
            .Not.Nullable()
            .CustomSqlType("uuid");

        Map(x => x.CreatedAt).DateTime();
        Map(x => x.UpdatedAt).DateTimeNullable();
        Map(x => x.DeletedAt).DateTimeNullable();
    }
}
