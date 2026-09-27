using PassKee.Orm.Entities;
using PassKee.Orm.Mapping.Common;

namespace PassKee.Orm.Mapping.Entities;

public class UserAccessTokenMapping : BaseGuidMappings<UserAccessTokenEntity>
{
    public UserAccessTokenMapping()
    {
        Table("user_access_tokens");

        Map(x => x.Token).Not.Nullable();
        Map(x => x.ExpirationTime).Not.Nullable();
        Map(x => x.CreatedAt).Not.Nullable();
        Map(x => x.UpdatedAt).Nullable();
        Map(x => x.DeletedAt).Nullable();

        References(x => x.User)
            .Column("user_id")
            .Fetch.Select()
            .LazyLoad()
            .Cascade.SaveUpdate();

        HasMany(x => x.JwtTokens)
            .KeyColumn("access_token_id")
            .Fetch.Select()
            .LazyLoad()
            .Cascade.SaveUpdate()
            .Inverse();
    }
}
