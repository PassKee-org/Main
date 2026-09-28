using PassKee.Orm.Core;

namespace PassKee.Orm.Entities.Vaults;

public enum CredentialType
{
    Login = 1,
    SecureNote = 2,
    Card = 3,
    Password = 4
}

public class CredentialEntity : AEntity
{
    public virtual Guid VaultId { get; set; }
    public virtual Guid? DirectoryId { get; set; }
    public virtual CredentialType Type { get; set; }
    public virtual byte[] EncryptedBody { get; set; } = null!;
}
