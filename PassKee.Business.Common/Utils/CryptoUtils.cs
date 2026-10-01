using System;
using System.Security.Cryptography;
using System.Text;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Macs;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Agreement;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

namespace PassKee.Business.Common.Utils;

public record UserKeyEnvelope(
    byte[] PublicKey,
    byte[] EncryptedPrivateKey,
    byte[] EncryptedVaultKey,
    byte[] PlainVaultKey
);

public record KdfParameters(
    int Iterations,
    int MemorySize,
    int Parallelism
);

public record ClientRegistrationData(
    byte[] SecretKey,
    byte[] AuthSalt,
    byte[] MasterKey,
    byte[] AuthHash,
    KdfParameters KdfParams,
    UserKeyEnvelope KeyEnvelope,
    string? SecretKeyString = null
);


public static class CryptoUtils
{
    private const int NonceSize = 12;
    private const int TagSize = 16;
    private const int MacSizeBits = 128;
    private const int SessionUnlockKdfIterations = 2;
    private const int SessionUnlockKdfMemorySize = 19456;
    private const int SessionUnlockKdfParallelism = 1;
    public const string AuthLoginContext = "auth_login";

    public const int DefaultKdfIterations = 1;
    public const int DefaultKdfMemorySize = 16384;
    public const int DefaultKdfParallelism = 1;

    public static byte[] GenerateRandomBytes(int length)
    {
        var random = new SecureRandom();
        byte[] bytes = new byte[length];
        random.NextBytes(bytes);
        return bytes;
    }

    public static byte[] GenerateArgon2idHash(
        byte[] password,
        byte[] salt,
        int iterations,
        int memorySize,
        int degreeOfParallelism,
        int hashLength
    )
    {
        var parameters = new Argon2Parameters.Builder(Argon2Parameters.Argon2id)
            .WithSalt(salt)
            .WithIterations(iterations)
            .WithMemoryAsKB(memorySize)
            .WithParallelism(degreeOfParallelism)
            .Build();

        var generator = new Argon2BytesGenerator();
        generator.Init(parameters);

        var result = new byte[hashLength];
        generator.GenerateBytes(password, result, 0, result.Length);
        return result;
    }

    public static byte[] GenerateHmacSha256(byte[] key, byte[] data)
    {
        var hmac = new HMac(new Sha256Digest());
        hmac.Init(new KeyParameter(key));

        var result = new byte[hmac.GetMacSize()];
        hmac.BlockUpdate(data, 0, data.Length);
        hmac.DoFinal(result, 0);
        return result;
    }

    /// <summary>
    /// Generates a Curve25519 (X25519) key pair.
    /// Public and private keys are each returned as raw 32-byte arrays.
    /// </summary>
    public static (byte[] PublicKey, byte[] PrivateKey) GenerateCurve25519KeyPair()
    {
        var generator = new X25519KeyPairGenerator();
        generator.Init(new X25519KeyGenerationParameters(new SecureRandom()));
        var pair = generator.GenerateKeyPair();

        var pubKey = (X25519PublicKeyParameters)pair.Public;
        var privKey = (X25519PrivateKeyParameters)pair.Private;

        return (pubKey.GetEncoded(), privKey.GetEncoded());
    }

    /// <summary>
    /// Encrypts data for a recipient's X25519 public key using Ephemeral ECDH + AES-256-GCM.
    /// Output format: [32 bytes Ephemeral Public Key][12 bytes Nonce][Ciphertext][16 bytes Auth Tag]
    /// </summary>
    public static byte[] EccEncrypt(byte[] recipientPublicKey, byte[] plaintext)
    {
        var (ephemeralPublicKey, ephemeralPrivateKey) = GenerateCurve25519KeyPair();
        var sharedSecret = Array.Empty<byte>();
        var derivedKey = Array.Empty<byte>();

        try
        {
            var agreement = new X25519Agreement();
            agreement.Init(new X25519PrivateKeyParameters(ephemeralPrivateKey, 0));
            sharedSecret = new byte[agreement.AgreementSize];
            agreement.CalculateAgreement(new X25519PublicKeyParameters(recipientPublicKey, 0), sharedSecret, 0);

            derivedKey = GenerateHmacSha256(sharedSecret, Encoding.UTF8.GetBytes("ecc_vault_encryption"));
            var encryptedPayload = AesGcmEncrypt(derivedKey, plaintext);

            var result = new byte[32 + encryptedPayload.Length];
            Buffer.BlockCopy(ephemeralPublicKey, 0, result, 0, 32);
            Buffer.BlockCopy(encryptedPayload, 0, result, 32, encryptedPayload.Length);
            return result;
        }
        finally
        {
            CryptographicOperations.ZeroMemory(ephemeralPrivateKey);
            CryptographicOperations.ZeroMemory(sharedSecret);
            CryptographicOperations.ZeroMemory(derivedKey);
        }
    }

    /// <summary>
    /// Decrypts data that was encrypted for an X25519 public key using the recipient's private key.
    /// </summary>
    public static byte[] EccDecrypt(byte[] recipientPrivateKey, byte[] encryptedData)
    {
        if (encryptedData == null || encryptedData.Length < 32 + NonceSize + TagSize)
        {
            throw new ArgumentException("Encrypted data is invalid or truncated.", nameof(encryptedData));
        }

        var ephemeralPublicKey = new byte[32];
        Buffer.BlockCopy(encryptedData, 0, ephemeralPublicKey, 0, 32);

        var agreement = new X25519Agreement();
        agreement.Init(new X25519PrivateKeyParameters(recipientPrivateKey, 0));
        var sharedSecret = new byte[agreement.AgreementSize];
        var derivedKey = Array.Empty<byte>();
        try
        {
            agreement.CalculateAgreement(new X25519PublicKeyParameters(ephemeralPublicKey, 0), sharedSecret, 0);
            derivedKey = GenerateHmacSha256(sharedSecret, Encoding.UTF8.GetBytes("ecc_vault_encryption"));

            var encryptedPayload = new byte[encryptedData.Length - 32];
            Buffer.BlockCopy(encryptedData, 32, encryptedPayload, 0, encryptedPayload.Length);

            return AesGcmDecrypt(derivedKey, encryptedPayload);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(sharedSecret);
            CryptographicOperations.ZeroMemory(derivedKey);
        }
    }

    /// <summary>
    /// Encrypts the user's Secret Key for temporary local session storage using a key derived from Master Password and Auth Salt.
    /// Allows unlocking the session using only the Master Password without re-entering the Secret Key.
    /// </summary>
    public static byte[] EncryptSecretKeyForSession(byte[] secretKey, string masterPassword, byte[] authSalt)
    {
        var sessionUnlockKey = DeriveSessionUnlockKey(masterPassword, authSalt);
        try
        {
            return AesGcmEncrypt(sessionUnlockKey, secretKey);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(sessionUnlockKey);
        }
    }

    /// <summary>
    /// Decrypts the user's Secret Key from temporary local session storage using the Master Password and Auth Salt.
    /// </summary>
    public static byte[] DecryptSecretKeyFromSession(byte[] encryptedSecretKey, string masterPassword, byte[] authSalt)
    {
        var sessionUnlockKey = DeriveSessionUnlockKey(masterPassword, authSalt);
        try
        {
            return AesGcmDecrypt(sessionUnlockKey, encryptedSecretKey);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(sessionUnlockKey);
        }
    }

    /// <summary>
    /// Encrypts the user's string Secret Key for temporary local session storage.
    /// </summary>
    public static byte[] EncryptSecretKeyForSession(string secretKey, string masterPassword, byte[] authSalt)
    {
        var secretKeyBytes = Encoding.UTF8.GetBytes(secretKey);
        try
        {
            return EncryptSecretKeyForSession(secretKeyBytes, masterPassword, authSalt);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(secretKeyBytes);
        }
    }

    /// <summary>
    /// Decrypts the user's string Secret Key from temporary local session storage.
    /// </summary>
    public static string DecryptSecretKeyStringFromSession(byte[] encryptedSecretKey, string masterPassword, byte[] authSalt)
    {
        var decryptedBytes = DecryptSecretKeyFromSession(encryptedSecretKey, masterPassword, authSalt);
        try
        {
            return Encoding.UTF8.GetString(decryptedBytes);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(decryptedBytes);
        }
    }

    private static byte[] DeriveSessionUnlockKey(string masterPassword, byte[] sessionSalt)
    {
        if (sessionSalt.Length < 16)
        {
            throw new ArgumentException("Session unlock salt must be at least 16 bytes.", nameof(sessionSalt));
        }

        var passwordBytes = Encoding.UTF8.GetBytes(masterPassword + ":session_unlock");
        try
        {
            return GenerateArgon2idHash(
                passwordBytes,
                sessionSalt,
                SessionUnlockKdfIterations,
                SessionUnlockKdfMemorySize,
                SessionUnlockKdfParallelism,
                32);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(passwordBytes);
        }
    }


    /// <summary>
    /// Encrypts data using AES-256-GCM.
    /// Format: [12 bytes Nonce][Ciphertext][16 bytes Auth Tag]
    /// </summary>
    public static byte[] AesGcmEncrypt(byte[] key, byte[] data)
    {
        var nonce = GenerateRandomBytes(NonceSize);
        var cipher = new GcmBlockCipher(new AesEngine());
        var parameters = new AeadParameters(new KeyParameter(key), MacSizeBits, nonce);
        cipher.Init(true, parameters);

        var output = new byte[cipher.GetOutputSize(data.Length)];
        var len = cipher.ProcessBytes(data, 0, data.Length, output, 0);
        cipher.DoFinal(output, len);

        var result = new byte[NonceSize + output.Length];
        Buffer.BlockCopy(nonce, 0, result, 0, NonceSize);
        Buffer.BlockCopy(output, 0, result, NonceSize, output.Length);
        return result;
    }

    /// <summary>
    /// Decrypts data using AES-256-GCM.
    /// Expects input in format: [12 bytes Nonce][Ciphertext][16 bytes Auth Tag]
    /// </summary>
    public static byte[] AesGcmDecrypt(byte[] key, byte[] encryptedData)
    {
        if (encryptedData == null || encryptedData.Length < NonceSize + TagSize)
        {
            throw new ArgumentException("Encrypted data is invalid or truncated.", nameof(encryptedData));
        }

        var nonce = new byte[NonceSize];
        Buffer.BlockCopy(encryptedData, 0, nonce, 0, NonceSize);

        var cipherTextAndTag = new byte[encryptedData.Length - NonceSize];
        Buffer.BlockCopy(encryptedData, NonceSize, cipherTextAndTag, 0, cipherTextAndTag.Length);

        var cipher = new GcmBlockCipher(new AesEngine());
        var parameters = new AeadParameters(new KeyParameter(key), MacSizeBits, nonce);
        cipher.Init(false, parameters);

        var plaintext = new byte[cipher.GetOutputSize(cipherTextAndTag.Length)];
        var len = cipher.ProcessBytes(cipherTextAndTag, 0, cipherTextAndTag.Length, plaintext, 0);
        cipher.DoFinal(plaintext, len);
        return plaintext;
    }

    /// <summary>
    /// Derives the client-side Master Key from Password, Secret Key, and Salt using Argon2id.
    /// </summary>
    public static byte[] DeriveMasterKey(
        string password,
        byte[] secretKey,
        byte[] salt,
        int iterations = DefaultKdfIterations,
        int memorySize = DefaultKdfMemorySize,
        int parallelism = DefaultKdfParallelism
    )
    {
        var passwordBytes = Encoding.UTF8.GetBytes(password);
        var combinedPassword = new byte[passwordBytes.Length + secretKey.Length];
        try
        {
            Buffer.BlockCopy(passwordBytes, 0, combinedPassword, 0, passwordBytes.Length);
            Buffer.BlockCopy(secretKey, 0, combinedPassword, passwordBytes.Length, secretKey.Length);

            return GenerateArgon2idHash(combinedPassword, salt, iterations, memorySize, parallelism, 32);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(passwordBytes);
            CryptographicOperations.ZeroMemory(combinedPassword);
        }
    }

    /// <summary>
    /// Derives the client-side Master Key directly from Password and string Secret Key and Salt using Argon2id.
    /// Does not require Base64 conversion.
    /// </summary>
    public static byte[] DeriveMasterKey(
        string password,
        string secretKey,
        byte[] salt,
        int iterations = DefaultKdfIterations,
        int memorySize = DefaultKdfMemorySize,
        int parallelism = DefaultKdfParallelism
    )
    {
        var secretKeyBytes = ParseSecretKey(secretKey);
        try
        {
            return DeriveMasterKey(password, secretKeyBytes, salt, iterations, memorySize, parallelism);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(secretKeyBytes);
        }
    }

    /// <summary>
    /// Calculates the client-side Auth Hash from the Master Key.
    /// </summary>
    public static byte[] ComputeAuthHash(byte[] masterKey)
    {
        return GenerateHmacSha256(masterKey, Encoding.UTF8.GetBytes(AuthLoginContext));
    }

    /// <summary>
    /// Generates Curve25519 (X25519) KeyPair and Vault Key and encrypts them with the Master Key.
    /// </summary>
    public static UserKeyEnvelope GenerateUserKeyEnvelope(byte[] masterKey)
    {
        var (publicKey, privateKey) = GenerateCurve25519KeyPair();
        var vaultKey = GenerateRandomBytes(32);
        try
        {
            var encryptedPrivateKey = AesGcmEncrypt(masterKey, privateKey);
            var encryptedVaultKey = AesGcmEncrypt(masterKey, vaultKey);

            return new UserKeyEnvelope(publicKey, encryptedPrivateKey, encryptedVaultKey, vaultKey);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(privateKey);
        }
    }

    /// <summary>
    /// Shared helper to prepare all cryptographic data required for user registration from a string Secret Key.
    /// </summary>
    public static ClientRegistrationData PrepareClientRegistration(
        string password,
        string? secretKey,
        byte[]? authSalt = null,
        int iterations = DefaultKdfIterations,
        int memorySize = DefaultKdfMemorySize,
        int parallelism = DefaultKdfParallelism
    )
    {
        var keyString = !string.IsNullOrWhiteSpace(secretKey) ? secretKey.Trim() : GenerateSecretKeyString();
        var keyBytes = ParseSecretKey(keyString);
        authSalt ??= GenerateRandomBytes(32);
        var kdfParams = new KdfParameters(iterations, memorySize, parallelism);

        var masterKey = DeriveMasterKey(password, keyBytes, authSalt, iterations, memorySize, parallelism);
        var authHash = ComputeAuthHash(masterKey);
        var keyEnvelope = GenerateUserKeyEnvelope(masterKey);

        return new ClientRegistrationData(keyBytes, authSalt, masterKey, authHash, kdfParams, keyEnvelope, keyString);
    }

    /// <summary>
    /// Shared helper to prepare all cryptographic data required for user registration.
    /// Eliminates boilerplate between Web, Tests, and external clients.
    /// </summary>
    public static ClientRegistrationData PrepareClientRegistration(
        string password,
        byte[]? secretKey = null,
        byte[]? authSalt = null,
        int iterations = DefaultKdfIterations,
        int memorySize = DefaultKdfMemorySize,
        int parallelism = DefaultKdfParallelism
    )
    {
        secretKey ??= GenerateRandomBytes(16);
        authSalt ??= GenerateRandomBytes(32);
        var kdfParams = new KdfParameters(iterations, memorySize, parallelism);

        var masterKey = DeriveMasterKey(password, secretKey, authSalt, iterations, memorySize, parallelism);
        var authHash = ComputeAuthHash(masterKey);
        var keyEnvelope = GenerateUserKeyEnvelope(masterKey);

        return new ClientRegistrationData(secretKey, authSalt, masterKey, authHash, kdfParams, keyEnvelope);
    }

    /// <summary>
    /// Computes the server-side double hash from the client's Auth Hash and Auth Salt using Argon2id.
    /// </summary>
    public static byte[] ComputeServerHash(byte[] authHash, byte[] authSalt)
    {
        return GenerateArgon2idHash(authHash, authSalt, DefaultKdfIterations, DefaultKdfMemorySize, DefaultKdfParallelism, 32);
    }

    /// <summary>
    /// Generates a human-readable high-entropy Secret Key formatted as PK-XXXX-XXXX-XXXX-XXXX-XXXX-XXXX.
    /// </summary>
    public static string GenerateSecretKeyString()
    {
        return SecurityUtil.GenerateSecretKey();
    }

    /// <summary>
    /// Parses a Secret Key string into raw bytes for master key derivation.
    /// Handles both formatted string keys (UTF-8 encoding) and legacy 16-byte Base64 keys.
    /// </summary>
    public static byte[] ParseSecretKey(string secretKey)
    {
        if (string.IsNullOrWhiteSpace(secretKey))
        {
            throw new ArgumentException("Secret key cannot be empty.", nameof(secretKey));
        }

        var trimmed = secretKey.Trim();

        var compactKey = new StringBuilder(trimmed.Length);
        foreach (var character in trimmed)
        {
            if (character != '-' && !char.IsWhiteSpace(character))
            {
                compactKey.Append(char.ToUpperInvariant(character));
            }
        }

        var normalizedKey = compactKey.ToString();
        if (normalizedKey.Length == 26 && normalizedKey.StartsWith("PK", StringComparison.Ordinal))
        {
            const string alphabet = "0123456789ABCDEFGHJKMNPQRSTVWXYZ";
            var isFormattedKey = true;
            for (var index = 2; index < normalizedKey.Length; index++)
            {
                if (!alphabet.Contains(normalizedKey[index]))
                {
                    isFormattedKey = false;
                    break;
                }
            }

            if (isFormattedKey)
            {
                var formattedKey = new StringBuilder("PK", 32);
                for (var index = 2; index < normalizedKey.Length; index += 4)
                {
                    formattedKey.Append('-').Append(normalizedKey, index, 4);
                }

                return Encoding.UTF8.GetBytes(formattedKey.ToString());
            }
        }

        // Check if it's a legacy Base64 16-byte key (e.g. 24 chars, valid Base64)
        if (!trimmed.StartsWith("PK-", StringComparison.OrdinalIgnoreCase) && Base64Utils.IsValidBase64(trimmed))
        {
            try
            {
                var bytes = Convert.FromBase64String(trimmed);
                if (bytes.Length == 16)
                {
                    return bytes;
                }
            }
            catch
            {
                // Not valid Base64, fall back to UTF-8
            }
        }

        // String-based key: UTF-8 bytes of the key string
        return Encoding.UTF8.GetBytes(trimmed);
    }

    /// <summary>
    /// Verifies the client's Auth Hash against the stored server hash in constant time.
    /// </summary>
    public static bool VerifyServerHash(byte[] authHash, byte[] authSalt, byte[] expectedServerHash)
    {
        var computedServerHash = ComputeServerHash(authHash, authSalt);
        return CryptographicOperations.FixedTimeEquals(computedServerHash, expectedServerHash);
    }
}
