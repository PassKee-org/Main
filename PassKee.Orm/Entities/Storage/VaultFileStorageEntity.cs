namespace PassKee.Orm.Entities.Storage;

public class VaultFileStorageEntity : FileStorageEntity
{
    public virtual Guid VaultId { get; set; }
}
