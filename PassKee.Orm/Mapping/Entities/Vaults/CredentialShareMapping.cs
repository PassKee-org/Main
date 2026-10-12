using PassKee.Orm.Entities.Vaults;
using PassKee.Orm.Extensions;
using PassKee.Orm.Mapping.Common;

namespace PassKee.Orm.Mapping.Entities.Vaults;

public class CredentialShareMapping : BaseGuidMappings<CredentialShareEntity>
{
    public CredentialShareMapping()
    {
        Table("credential_shares");
        Map(x => x.UserId).Column("user_id").Not.Nullable();
        Map(x => x.CredentialId).Column("credential_id").Nullable();
        Map(x => x.Token).Column("token").Length(128).Not.Nullable();
        Map(x => x.Type).Column("type").CustomType<int>().Not.Nullable();
        Map(x => x.EncryptedBody).Column("encrypted_body").Not.Nullable();
        Map(x => x.ExpiresAt).Column("expires_at").DateTimeNullable();
        Map(x => x.MaxViews).Column("max_views").Nullable();
        Map(x => x.ViewCount).Column("view_count").Not.Nullable();
    }
}

