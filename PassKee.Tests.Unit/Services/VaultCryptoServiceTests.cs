using System;
using System.Collections.Generic;
using PassKee.Api.Shared.Models.Vaults;
using PassKee.Api.Shared.Models.Vaults.Enums;
using PassKee.Api.Shared.Models.Vaults.Payloads;
using PassKee.Business.Common.Utils;
using PassKee.Web.Core.Services.Vaults;
using Xunit;

namespace PassKee.Tests.Unit.Services;

public class VaultCryptoServiceTests
{
    private readonly VaultCryptoService _service = new();

    [Fact]
    public void Should_Encrypt_And_Decrypt_VaultKey_Using_ECC_KeyPair()
    {
        // 1. Generate ECC (Curve25519) key pair
        var (publicKey, privateKey) = CryptoUtils.GenerateCurve25519KeyPair();
        var originalVaultKey = CryptoUtils.GenerateRandomBytes(32);

        // 2. Encrypt with ECC public key
        var encryptedVaultKey = _service.EncryptVaultKey(originalVaultKey, publicKey);
        Assert.NotNull(encryptedVaultKey);
        Assert.True(encryptedVaultKey.Length > 32);

        // 3. Decrypt with ECC private key
        var decryptedVaultKey = _service.DecryptVaultKey(encryptedVaultKey, privateKey);
        Assert.Equal(originalVaultKey, decryptedVaultKey);
    }

    [Fact]
    public void Should_Encrypt_And_Decrypt_Directory_Name()
    {
        var vaultKey = CryptoUtils.GenerateRandomBytes(32);
        var dirName = "Personal Finance & Banking";

        var encrypted = _service.EncryptDirectoryName(dirName, vaultKey);
        Assert.NotNull(encrypted);
        Assert.NotEqual(dirName, System.Text.Encoding.UTF8.GetString(encrypted));

        var decrypted = _service.DecryptDirectoryName(encrypted, vaultKey);
        Assert.Equal(dirName, decrypted);
    }

    [Fact]
    public void Should_Decrypt_Directories_List()
    {
        var vaultKey = CryptoUtils.GenerateRandomBytes(32);
        var vaultId = Guid.NewGuid();

        var dirs = new List<DirectoryDto>
        {
            new() { Id = Guid.NewGuid(), VaultId = vaultId, EncryptedName = _service.EncryptDirectoryName("Work", vaultKey) },
            new() { Id = Guid.NewGuid(), VaultId = vaultId, EncryptedName = _service.EncryptDirectoryName("Personal", vaultKey) }
        };

        var decrypted = _service.DecryptDirectories(dirs, vaultKey);
        Assert.Equal(2, decrypted.Count);
        Assert.Equal("Work", decrypted[0].Name);
        Assert.Equal("Personal", decrypted[1].Name);
    }

    [Fact]
    public void Should_Encrypt_And_Decrypt_LoginCredentialPayload()
    {
        var vaultKey = CryptoUtils.GenerateRandomBytes(32);
        var payload = new LoginCredentialPayload
        {
            Title = "GitHub Account",
            Username = "developer@passkee.io",
            Password = "P@ssw0rd#SuperSecure!",
            Website = "https://github.com"
        };

        var encrypted = _service.EncryptCredentialPayload(payload, vaultKey);
        Assert.NotNull(encrypted);

        var decrypted = _service.DecryptCredentialPayload(encrypted, CredentialType.Login, vaultKey);
        Assert.IsType<LoginCredentialPayload>(decrypted);
        var login = (LoginCredentialPayload)decrypted;
        Assert.Equal(payload.Title, login.Title);
        Assert.Equal(payload.Username, login.Username);
        Assert.Equal(payload.Password, login.Password);
        Assert.Equal(payload.Website, login.Website);
    }

    [Fact]
    public void Should_Encrypt_And_Decrypt_CardCredentialPayload()
    {
        var vaultKey = CryptoUtils.GenerateRandomBytes(32);
        var payload = new CardCredentialPayload
        {
            Title = "Debit Card",
            CardholderName = "John Doe",
            CardNumber = "4111222233334444",
            ExpirationDate = "12/28",
            Cvv = "123",
            Pin = "9876"
        };

        var encrypted = _service.EncryptCredentialPayload(payload, vaultKey);
        var decrypted = _service.DecryptCredentialPayload(encrypted, CredentialType.Card, vaultKey);

        Assert.IsType<CardCredentialPayload>(decrypted);
        var card = (CardCredentialPayload)decrypted;
        Assert.Equal(payload.Title, card.Title);
        Assert.Equal(payload.CardNumber, card.CardNumber);
        Assert.Equal(payload.Cvv, card.Cvv);
    }

    [Fact]
    public void Should_Encrypt_And_Decrypt_PasswordCredentialPayload()
    {
        var vaultKey = CryptoUtils.GenerateRandomBytes(32);
        var payload = new PasswordCredentialPayload
        {
            Title = "Wi-Fi Router Password",
            Password = "SuperSecretWiFiKey99"
        };

        var encrypted = _service.EncryptCredentialPayload(payload, vaultKey);
        var decrypted = _service.DecryptCredentialPayload(encrypted, CredentialType.Password, vaultKey);

        Assert.IsType<PasswordCredentialPayload>(decrypted);
        var pass = (PasswordCredentialPayload)decrypted;
        Assert.Equal(payload.Title, pass.Title);
        Assert.Equal(payload.Password, pass.Password);
    }

    [Fact]
    public void Should_Encrypt_And_Decrypt_SecureNoteCredentialPayload()
    {
        var vaultKey = CryptoUtils.GenerateRandomBytes(32);
        var payload = new SecureNoteCredentialPayload
        {
            Title = "Recovery Seeds",
            Notes = "apple banana orange cherry grape mango pineapple watermelon"
        };

        var encrypted = _service.EncryptCredentialPayload(payload, vaultKey);
        var decrypted = _service.DecryptCredentialPayload(encrypted, CredentialType.SecureNote, vaultKey);

        Assert.IsType<SecureNoteCredentialPayload>(decrypted);
        var note = (SecureNoteCredentialPayload)decrypted;
        Assert.Equal(payload.Title, note.Title);
        Assert.Equal(payload.Notes, note.Notes);
    }

    [Fact]
    public void Should_Encrypt_And_Decrypt_SecretKey_For_Session()
    {
        var secretKey = CryptoUtils.GenerateRandomBytes(16);
        var masterPassword = "MyMasterPassword#2026";
        var sessionSalt = CryptoUtils.GenerateRandomBytes(32);

        var encryptedSecretKey = CryptoUtils.EncryptSecretKeyForSession(secretKey, masterPassword, sessionSalt);
        Assert.NotNull(encryptedSecretKey);

        var decryptedSecretKey = CryptoUtils.DecryptSecretKeyFromSession(encryptedSecretKey, masterPassword, sessionSalt);
        Assert.Equal(secretKey, decryptedSecretKey);

        // Incorrect password should fail decryption
        Assert.ThrowsAny<Exception>(() =>
            CryptoUtils.DecryptSecretKeyFromSession(encryptedSecretKey, "WrongPassword#9999", sessionSalt));

        Assert.ThrowsAny<Exception>(() =>
            CryptoUtils.DecryptSecretKeyFromSession(encryptedSecretKey, masterPassword, CryptoUtils.GenerateRandomBytes(32)));
    }
}
