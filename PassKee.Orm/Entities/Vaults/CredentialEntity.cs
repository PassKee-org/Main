using PassKee.Business.Common.Constants;
using PassKee.Orm.Core;

namespace PassKee.Orm.Entities.Vaults;

public class CredentialEntity : AEntity
{
    public virtual Guid VaultId { get; set; }
    public virtual Guid? DirectoryId { get; set; }
    public virtual CredentialType Type { get; set; }
    public virtual byte[] EncryptedBody { get; set; } = null!;
}

