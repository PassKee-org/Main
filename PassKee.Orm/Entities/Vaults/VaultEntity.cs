using PassKee.Orm.Core;

namespace PassKee.Orm.Entities.Vaults;

public class VaultEntity : AEntity
{
    public virtual Guid UserId { get; set; }
    public virtual string Name { get; set; } = null!;

    /// <summary>
    /// AES-256 vault key encrypted with the user's X25519 (Curve25519) ECC public key.
    /// Stored as raw binary (PostgreSQL bytea).
    /// </summary>
    public virtual byte[]? EncryptedVaultKey { get; set; }
}

