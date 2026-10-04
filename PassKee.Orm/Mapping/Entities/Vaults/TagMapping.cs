using PassKee.Orm.Entities.Vaults;
using PassKee.Orm.Mapping.Common;

namespace PassKee.Orm.Mapping.Entities.Vaults;

public class TagMapping : BaseGuidMappings<TagEntity>
{
    public TagMapping()
    {
        Table("tags");
        Map(x => x.VaultId).Column("vault_id").Not.Nullable();
        Map(x => x.EncryptedName).Column("encrypted_name").Not.Nullable();
    }
}
