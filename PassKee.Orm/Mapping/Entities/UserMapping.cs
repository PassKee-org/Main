using PassKee.Orm.Entities;
using PassKee.Orm.Mapping.Common;

namespace PassKee.Orm.Mapping.Entities;

public class UserMapping : BaseGuidMappings<UserEntity>
{
    public UserMapping()
    {
        Table("users");
        Map(x => x.Email).Not.Nullable();
        Map(x => x.AuthSalt).Nullable();
        Map(x => x.ServerHash).Nullable();
        Map(x => x.UserPublicKey).Nullable();
        Map(x => x.EncryptedUserPrivateKey).Nullable();
        Map(x => x.EncryptedUserVaultKey).Nullable();

        HasOne(x => x.KdfParams)
            .PropertyRef("User")
            .Fetch.Select()
            .LazyLoad()
            .Cascade.SaveUpdate();
    }
}
