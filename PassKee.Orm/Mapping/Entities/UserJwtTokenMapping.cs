using PassKee.Orm.Entities;
using PassKee.Orm.Mapping.Common;

namespace PassKee.Orm.Mapping.Entities;

public class UserJwtTokenMapping : BaseGuidMappings<UserJwtTokenEntity>
{
    public UserJwtTokenMapping()
    {
        Table("user_jwt_tokens");

        Map(x => x.Token).Not.Nullable().Length(2056);
        Map(x => x.ExpirationTime).Not.Nullable();

        References(x => x.AccessToken)
            .Column("access_token_id")
            .Fetch.Select()
            .LazyLoad()
            .Cascade.SaveUpdate();
    }
}
