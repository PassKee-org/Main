using PassKee.Orm.Entities.Vaults;
using PassKee.Orm.Mapping.Common;

namespace PassKee.Orm.Mapping.Entities.Vaults;

public class VaultMapping : BaseGuidMappings<VaultEntity>
{
    public VaultMapping()
    {
        Table("vaults");
        Map(x => x.UserId).Column("user_id").Not.Nullable();
        Map(x => x.Name).Column("name").Not.Nullable();
        Map(x => x.EncryptedVaultKey).Column("encrypted_vault_key").Nullable();
    }
}
