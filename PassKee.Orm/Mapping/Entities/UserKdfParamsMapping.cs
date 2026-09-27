using PassKee.Orm.Entities;
using PassKee.Orm.Extensions;
using PassKee.Orm.Mapping.Common;

namespace PassKee.Orm.Mapping.Entities;

public class UserKdfParamsMapping : BaseGuidMappings<UserKdfParamsEntity>
{
    public UserKdfParamsMapping()
    {
        Table("user_kdf_params");

        Map(x => x.Iterations).Not.Nullable();
        Map(x => x.MemorySize).Not.Nullable();
        Map(x => x.Parallelism).Not.Nullable();

        Map(x => x.CreatedAt).DateTime();
        Map(x => x.UpdatedAt).DateTimeNullable();
        Map(x => x.DeletedAt).DateTimeNullable();

        References(x => x.User)
            .Column("user_id")
            .Unique()
            .Fetch.Select()
            .LazyLoad()
            .Cascade.None();
    }
}
