using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Macs;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;

namespace PassKee.Business.Common.Utils;

public record UserKeyEnvelope(
    byte[] PublicKey,
    byte[] EncryptedPrivateKey,
    byte[] EncryptedVaultKey,
    byte[] PlainVaultKey
);

public record ClientRegistrationData(
    byte[] SecretKey,
    byte[] AuthSalt,
    byte[] MasterKey,
    byte[] AuthHash,
    string KdfParams,
    UserKeyEnvelope KeyEnvelope
);

public static class CryptoUtils
{
    private const int NonceSize = 12;
    private const int TagSize = 16;
    private const int MacSizeBits = 128;
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

    public static (byte[] PublicKey, byte[] PrivateKey) GenerateRsaKeyPair(int keySize = 2048)
    {
        var keyGenerationParameters = new KeyGenerationParameters(new SecureRandom(), keySize);
        var generator = new RsaKeyPairGenerator();
        generator.Init(keyGenerationParameters);
        var pair = generator.GenerateKeyPair();

        var privateKeyInfo = PrivateKeyInfoFactory.CreatePrivateKeyInfo(pair.Private);
        var privateKeyBytes = privateKeyInfo.ToAsn1Object().GetDerEncoded();

        var publicKeyInfo = SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(pair.Public);
        var publicKeyBytes = publicKeyInfo.ToAsn1Object().GetDerEncoded();

        return (publicKeyBytes, privateKeyBytes);
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
        Buffer.BlockCopy(passwordBytes, 0, combinedPassword, 0, passwordBytes.Length);
        Buffer.BlockCopy(secretKey, 0, combinedPassword, passwordBytes.Length, secretKey.Length);

        return GenerateArgon2idHash(combinedPassword, salt, iterations, memorySize, parallelism, 32);
    }

    /// <summary>
    /// Calculates the client-side Auth Hash from the Master Key.
    /// </summary>
    public static byte[] ComputeAuthHash(byte[] masterKey)
    {
        return GenerateHmacSha256(masterKey, Encoding.UTF8.GetBytes(AuthLoginContext));
    }

    /// <summary>
    /// Generates RSA KeyPair and Vault Key and encrypts them with the Master Key.
    /// </summary>
    public static UserKeyEnvelope GenerateUserKeyEnvelope(byte[] masterKey)
    {
        var (publicKey, privateKey) = GenerateRsaKeyPair();
        var vaultKey = GenerateRandomBytes(32);

        var encryptedPrivateKey = AesGcmEncrypt(masterKey, privateKey);
        var encryptedVaultKey = AesGcmEncrypt(masterKey, vaultKey);

        return new UserKeyEnvelope(publicKey, encryptedPrivateKey, encryptedVaultKey, vaultKey);
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
        var kdfParams = JsonSerializer.Serialize(new { iterations, memorySize, parallelism });

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
    /// Verifies the client's Auth Hash against the stored server hash in constant time.
    /// </summary>
    public static bool VerifyServerHash(byte[] authHash, byte[] authSalt, byte[] expectedServerHash)
    {
        var computedServerHash = ComputeServerHash(authHash, authSalt);
        return CryptographicOperations.FixedTimeEquals(computedServerHash, expectedServerHash);
    }
}
