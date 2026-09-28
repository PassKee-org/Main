using PassKee.Orm.Entities.Vaults;
using PassKee.Orm.Mapping.Common;

namespace PassKee.Orm.Mapping.Entities.Vaults;

public class CredentialMapping : BaseGuidMappings<CredentialEntity>
{
    public CredentialMapping()
    {
        Table("credentials");
        Map(x => x.VaultId).Column("vault_id").Not.Nullable();
        Map(x => x.DirectoryId).Column("directory_id").Nullable();
        Map(x => x.Type).Column("type").CustomType<int>().Not.Nullable();
        Map(x => x.EncryptedBody).Column("encrypted_body").Not.Nullable();
    }
}
