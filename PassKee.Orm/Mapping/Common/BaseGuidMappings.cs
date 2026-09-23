using Domain.Abstractions;
using FluentNHibernate.Mapping;
using PassKee.Orm.Core.Generators;

namespace PassKee.Orm.Mapping.Common;

public class BaseGuidMappings<T>: ClassMap<T> where T : IEntity
{
    public BaseGuidMappings()
    {
        Id(x => x.Id)
            .GeneratedBy.Custom<GuidV7Generator>()
            .Unique()
            .Not.Nullable()
            .CustomSqlType("uuid");
    }
}
