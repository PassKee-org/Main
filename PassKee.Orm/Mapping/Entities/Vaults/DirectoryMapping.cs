using PassKee.Orm.Entities.Vaults;
using PassKee.Orm.Mapping.Common;

namespace PassKee.Orm.Mapping.Entities.Vaults;

public class DirectoryMapping : BaseGuidMappings<DirectoryEntity>
{
    public DirectoryMapping()
    {
        Table("directories");
        Map(x => x.VaultId).Column("vault_id").Not.Nullable();
        Map(x => x.ParentDirectoryId).Column("parent_directory_id").Nullable();
        Map(x => x.EncryptedName).Column("encrypted_name").Not.Nullable();
    }
}
