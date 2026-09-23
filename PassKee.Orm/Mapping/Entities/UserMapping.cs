using PassKee.Orm.Entities;
using PassKee.Orm.Mapping.Common;

namespace PassKee.Orm.Mapping.Entities;

public class UserMapping : BaseGuidMappings<UserEntity>
{
    public UserMapping()
    {
        Table("users");
        Map(x => x.Email).Not.Nullable();
    }
}

