using PassKee.Orm.Core;

namespace PassKee.Orm.Entities;

/// <summary>
/// Represents a user in the PassKee system with cryptographic key material.
/// All sensitive cryptographic keys and hashes are stored in raw binary format.
/// </summary>
public class UserEntity : AEntity
{
    /// <summary>
    /// User's normalized email address used for identification.
    /// </summary>
    public virtual string Email { get; set; } = null!;

    /// <summary>
    /// Cryptographic salt used for client-side Master Key derivation (Argon2id) and server-side authentication hashing.
    /// Stored as a raw byte array.
    /// </summary>
    public virtual byte[]? AuthSalt { get; set; }

    /// <summary>
    /// Double-hashed authentication verifier: Argon2id(AuthHash).
    /// Used by the server to verify login without ever knowing or storing the client's AuthHash or Master Key.
    /// Stored as a raw byte array.
    /// </summary>
    public virtual byte[]? ServerHash { get; set; }

    /// <summary>
    /// Parameters used by the Argon2id KDF algorithm (iterations, memorySize, parallelism).
    /// </summary>
    public virtual UserKdfParamsEntity? KdfParams { get; set; }

    /// <summary>
    /// User's asymmetric Curve25519 (X25519) public key (32 bytes raw binary).
    /// Publicly distributed so other users can encrypt shared vault keys for this user.
    /// </summary>
    public virtual byte[]? UserPublicKey { get; set; }

    /// <summary>
    /// User's asymmetric Curve25519 (X25519) private key encrypted on the client side using the Master Key (AES-GCM).
    /// Stored as a raw byte array.
    /// </summary>
    public virtual byte[]? EncryptedUserPrivateKey { get; set; }

    /// <summary>
    /// User's personal vault encryption key encrypted on the client side using the Master Key (AES-GCM).
    /// Stored as a raw byte array.
    /// </summary>
    public virtual byte[]? EncryptedUserVaultKey { get; set; }

    /// <summary>
    /// Active and historic access tokens for this user.
    /// </summary>
    public virtual ICollection<UserAccessTokenEntity> AccessTokens { get; set; } = new List<UserAccessTokenEntity>();
}
