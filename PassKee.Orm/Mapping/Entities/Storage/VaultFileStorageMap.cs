using FluentNHibernate.Mapping;
using PassKee.Orm.Entities.Storage;

namespace PassKee.Orm.Mapping.Entities.Storage;

public class VaultFileStorageMap : SubclassMap<VaultFileStorageEntity>
{
    public VaultFileStorageMap()
    {
        Table("vault_file_storage");
        KeyColumn("id");
        Map(x => x.VaultId).Not.Nullable().Index("idx_vault_file_storage_vault_id");
    }
}
