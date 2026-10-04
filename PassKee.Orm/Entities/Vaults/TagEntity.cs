using PassKee.Orm.Core;

namespace PassKee.Orm.Entities.Vaults;

public class TagEntity : AEntity
{
    public virtual Guid VaultId { get; set; }
    public virtual byte[] EncryptedName { get; set; } = null!;
}
