using System;
using PassKee.Business.Common.Constants;
using PassKee.Orm.Core;

namespace PassKee.Orm.Entities.Vaults;

/// <summary>
/// Represents a temporary or one-time share of a credential entry.
/// </summary>
public class CredentialShareEntity : AEntity
{
    public virtual Guid UserId { get; set; }
    public virtual Guid? CredentialId { get; set; }
    public virtual string Token { get; set; } = null!;
    public virtual CredentialType Type { get; set; }

    /// <summary>
    /// AES-256-GCM encrypted payload of the credential entry using the per-share key.
    /// Format: [12 bytes Nonce][Ciphertext][16 bytes Auth Tag].
    /// Stored as raw binary bytes.
    /// </summary>
    public virtual byte[] EncryptedBody { get; set; } = null!;

    public virtual DateTime? ExpiresAt { get; set; }
    public virtual int? MaxViews { get; set; }
    public virtual int ViewCount { get; set; }
}

