using PassKee.Orm.Core;

namespace PassKee.Orm.Entities.Vaults;

public class DirectoryEntity : AEntity
{
    public virtual Guid VaultId { get; set; }
    public virtual Guid? ParentDirectoryId { get; set; }
    public virtual byte[] EncryptedName { get; set; } = null!;
}
