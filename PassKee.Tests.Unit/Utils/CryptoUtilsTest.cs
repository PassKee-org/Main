using System;
using System.Text;
using Org.BouncyCastle.Crypto;
using PassKee.Business.Common.Utils;
using Xunit;

namespace PassKee.Tests.Unit.Utils;

public class CryptoUtilsTest
{
    [Fact]
    public void GenerateRandomBytes_ShouldReturnRequestedLength()
    {
        var bytes = CryptoUtils.GenerateRandomBytes(32);
        Assert.NotNull(bytes);
        Assert.Equal(32, bytes.Length);

        var bytes2 = CryptoUtils.GenerateRandomBytes(32);
        Assert.NotEqual(bytes, bytes2);
    }

    [Fact]
    public void GenerateArgon2idHash_ShouldProduceDeterministicOutput()
    {
        var password = Encoding.UTF8.GetBytes("P@ssw0rd123!");
        var salt = Encoding.UTF8.GetBytes("somesalt12345678");

        var hash1 = CryptoUtils.GenerateArgon2idHash(password, salt, 1, 1024, 1, 32);
        var hash2 = CryptoUtils.GenerateArgon2idHash(password, salt, 1, 1024, 1, 32);

        Assert.Equal(32, hash1.Length);
        Assert.Equal(hash1, hash2);
    }

    [Fact]
    public void GenerateArgon2idHash_DifferentInputs_ShouldProduceDifferentOutput()
    {
        var password = Encoding.UTF8.GetBytes("P@ssw0rd123!");
        var salt1 = Encoding.UTF8.GetBytes("somesalt12345678");
        var salt2 = Encoding.UTF8.GetBytes("othersalt1234567");

        var hash1 = CryptoUtils.GenerateArgon2idHash(password, salt1, 1, 1024, 1, 32);
        var hash2 = CryptoUtils.GenerateArgon2idHash(password, salt2, 1, 1024, 1, 32);

        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public void GenerateHmacSha256_ShouldProduceConsistentOutput()
    {
        var key = Encoding.UTF8.GetBytes("my-secret-key-32-bytes-length-12");
        var data = Encoding.UTF8.GetBytes("auth_login");

        var mac1 = CryptoUtils.GenerateHmacSha256(key, data);
        var mac2 = CryptoUtils.GenerateHmacSha256(key, data);

        Assert.Equal(32, mac1.Length);
        Assert.Equal(mac1, mac2);
    }

    [Fact]
    public void GenerateCurve25519KeyPair_ShouldGenerateValid32ByteKeys()
    {
        var (publicKey, privateKey) = CryptoUtils.GenerateCurve25519KeyPair();

        Assert.NotNull(publicKey);
        Assert.Equal(32, publicKey.Length);
        Assert.NotNull(privateKey);
        Assert.Equal(32, privateKey.Length);
        Assert.NotEqual(publicKey, privateKey);
    }

    [Fact]
    public void AesGcmEncrypt_And_Decrypt_ShouldRoundtripSuccessfully()
    {
        var key = CryptoUtils.GenerateRandomBytes(32);
        var originalText = "Sensitive vault item data to protect!";
        var originalBytes = Encoding.UTF8.GetBytes(originalText);

        var encrypted = CryptoUtils.AesGcmEncrypt(key, originalBytes);
        Assert.NotNull(encrypted);
        Assert.True(encrypted.Length > originalBytes.Length);

        var decryptedBytes = CryptoUtils.AesGcmDecrypt(key, encrypted);
        var decryptedText = Encoding.UTF8.GetString(decryptedBytes);

        Assert.Equal(originalText, decryptedText);
    }

    [Fact]
    public void AesGcmDecrypt_WithTamperedData_ShouldThrow()
    {
        var key = CryptoUtils.GenerateRandomBytes(32);
        var originalBytes = Encoding.UTF8.GetBytes("Protected confidential payload");

        var encrypted = CryptoUtils.AesGcmEncrypt(key, originalBytes);
        // Tamper with the ciphertext
        encrypted[^1] ^= 0xFF;

        Assert.ThrowsAny<InvalidCipherTextException>(() =>
        {
            CryptoUtils.AesGcmDecrypt(key, encrypted);
        });
    }

    [Fact]
    public void AesGcmDecrypt_WithTruncatedData_ShouldThrow()
    {
        var key = CryptoUtils.GenerateRandomBytes(32);
        var tooShortData = new byte[10];

        Assert.Throws<ArgumentException>(() =>
        {
            CryptoUtils.AesGcmDecrypt(key, tooShortData);
        });
    }

    [Fact]
    public void DeriveMasterKey_And_ComputeAuthHash_ShouldBeDeterministic()
    {
        var password = "SecureMasterPassword#2026";
        var secretKey = CryptoUtils.GenerateRandomBytes(16);
        var authSalt = CryptoUtils.GenerateRandomBytes(32);

        var masterKey1 = CryptoUtils.DeriveMasterKey(password, secretKey, authSalt, 1, 1024, 1);
        var masterKey2 = CryptoUtils.DeriveMasterKey(password, secretKey, authSalt, 1, 1024, 1);
        Assert.Equal(masterKey1, masterKey2);

        var authHash1 = CryptoUtils.ComputeAuthHash(masterKey1);
        var authHash2 = CryptoUtils.ComputeAuthHash(masterKey2);
        Assert.Equal(authHash1, authHash2);
    }

    [Fact]
    public void PrepareClientRegistration_ShouldProduceFullConsistentRegistrationData()
    {
        var password = "SecureMasterPassword#2026";
        var regData = CryptoUtils.PrepareClientRegistration(password, iterations: 1, memorySize: 1024, parallelism: 1);

        Assert.NotNull(regData.SecretKey);
        Assert.Equal(16, regData.SecretKey.Length);
        Assert.NotNull(regData.AuthSalt);
        Assert.Equal(32, regData.AuthSalt.Length);
        Assert.NotNull(regData.MasterKey);
        Assert.NotNull(regData.AuthHash);
        Assert.NotNull(regData.KdfParams);
        Assert.Equal(1, regData.KdfParams.Iterations);
        Assert.Equal(1024, regData.KdfParams.MemorySize);
        Assert.Equal(1, regData.KdfParams.Parallelism);

        Assert.NotNull(regData.KeyEnvelope.PublicKey);
        Assert.Equal(32, regData.KeyEnvelope.PublicKey.Length);
        Assert.NotNull(regData.KeyEnvelope.EncryptedPrivateKey);
        Assert.NotNull(regData.KeyEnvelope.EncryptedVaultKey);
        Assert.NotNull(regData.KeyEnvelope.PlainVaultKey);

        // Verify that the encrypted private key and vault key can be decrypted with the master key
        var decryptedPrivateKey = CryptoUtils.AesGcmDecrypt(regData.MasterKey, regData.KeyEnvelope.EncryptedPrivateKey);
        Assert.Equal(32, decryptedPrivateKey.Length);

        var decryptedVaultKey = CryptoUtils.AesGcmDecrypt(regData.MasterKey, regData.KeyEnvelope.EncryptedVaultKey);
        Assert.Equal(regData.KeyEnvelope.PlainVaultKey, decryptedVaultKey);
    }

    [Fact]
    public void ComputeServerHash_And_VerifyServerHash_ShouldWorkCorrectly()
    {
        var authHash = CryptoUtils.GenerateRandomBytes(32);
        var authSalt = CryptoUtils.GenerateRandomBytes(32);

        var serverHash = CryptoUtils.ComputeServerHash(authHash, authSalt);
        Assert.NotNull(serverHash);
        Assert.Equal(32, serverHash.Length);

        Assert.True(CryptoUtils.VerifyServerHash(authHash, authSalt, serverHash));

        var invalidAuthHash = CryptoUtils.GenerateRandomBytes(32);
        Assert.False(CryptoUtils.VerifyServerHash(invalidAuthHash, authSalt, serverHash));
    }
}

