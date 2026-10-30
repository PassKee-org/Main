using System;
using System.Collections.Generic;
using System.Linq;
using PassKee.Api.Shared.Models.Storage;
using PassKee.Api.Shared.Models.Vaults.Payloads;
using PassKee.Business.Common.Constants;
using PassKee.Business.Common.Helpers;
using PassKee.Business.Common.Utils;
using PassKee.Web.Core.Services.Vaults;
using Xunit;

namespace PassKee.Tests.Unit.Web.Core.Services;

public class CredentialShareCryptoTests
{
    private readonly VaultCryptoService _service = new();

    [Fact]
    public void Should_Encrypt_And_Decrypt_Credential_Share_Using_Per_Share_Key()
    {
        var shareKey = CryptoUtils.GenerateRandomBytes(32);
        var originalPayload = new LoginCredentialPayload
        {
            Title = "My Netflix Account",
            Username = "user@example.com",
            Password = "SuperSecretPassword123!",
            Website = "https://netflix.com",
            Notes = "Family plan"
        };

        var encryptedBytes = _service.EncryptCredentialPayload(originalPayload, shareKey);
        Assert.NotNull(encryptedBytes);
        Assert.True(encryptedBytes.Length > 0);

        var decrypted = (LoginCredentialPayload)_service.DecryptCredentialPayload(encryptedBytes, CredentialType.Login, shareKey);
        Assert.NotNull(decrypted);
        Assert.Equal("My Netflix Account", decrypted.Title);
        Assert.Equal("user@example.com", decrypted.Username);
        Assert.Equal("SuperSecretPassword123!", decrypted.Password);
        Assert.Equal("https://netflix.com", decrypted.Website);
        Assert.Equal("Family plan", decrypted.Notes);
    }

    [Fact]
    public void Should_Fail_Decryption_When_Per_Share_Key_Is_Wrong()
    {
        var shareKey = CryptoUtils.GenerateRandomBytes(32);
        var wrongKey = CryptoUtils.GenerateRandomBytes(32);

        var payload = new PasswordCredentialPayload
        {
            Title = "WiFi Router",
            Password = "admin"
        };

        var encryptedBytes = _service.EncryptCredentialPayload(payload, shareKey);

        Assert.ThrowsAny<Exception>(() =>
        {
            _service.DecryptCredentialPayload(encryptedBytes, CredentialType.Password, wrongKey);
        });
    }

    [Fact]
    public void Should_Strip_Files_When_Preparing_Shared_Payload()
    {
        var filePayload = new FileCredentialPayload
        {
            Title = "Certificate with private key",
            File = new StoredFileDto { Id = Guid.NewGuid(), FileName = "cert.p12", Size = 1024 },
            AdditionalFields = new List<CredentialField>
            {
                new() { Type = CredentialFieldType.Text, Label = "Issuer", Value = "DigiCert" },
                new() { Type = CredentialFieldType.File, Label = "Backup Cert", File = new StoredFileDto { Id = Guid.NewGuid(), FileName = "backup.p12" } }
            },
            Sections = new List<CredentialSection>
            {
                new()
                {
                    Title = "Extras",
                    Fields = new List<CredentialField>
                    {
                        new() { Type = CredentialFieldType.Text, Label = "Pin", Value = "9999" },
                        new() { Type = CredentialFieldType.File, Label = "Keyfile", File = new StoredFileDto { Id = Guid.NewGuid(), FileName = "key.bin" } }
                    }
                }
            }
        };

        // Clone and strip files as done in ShareCredentialModal
        var json = JsonHelper.SerializeToString(filePayload);
        var cleanPayload = (FileCredentialPayload)JsonHelper.DeserializeObject(json, typeof(FileCredentialPayload))!;
        cleanPayload.File = null;
        cleanPayload.AdditionalFields = cleanPayload.AdditionalFields
            .Where(f => f.Type != CredentialFieldType.File)
            .ToList();
        foreach (var sec in cleanPayload.Sections)
        {
            sec.Fields = sec.Fields.Where(f => f.Type != CredentialFieldType.File).ToList();
        }

        Assert.Null(cleanPayload.File);
        Assert.Single(cleanPayload.AdditionalFields);
        Assert.Equal("Issuer", cleanPayload.AdditionalFields[0].Label);
        Assert.Single(cleanPayload.Sections[0].Fields);
        Assert.Equal("Pin", cleanPayload.Sections[0].Fields[0].Label);
    }

    [Fact]
    public void Should_Encode_And_Decode_Hex_Key_In_Url_Fragment()
    {
        var key = CryptoUtils.GenerateRandomBytes(32);
        var hex = Convert.ToHexString(key).ToLowerInvariant();

        Assert.Equal(64, hex.Length);
        var decoded = Convert.FromHexString(hex);
        Assert.Equal(key, decoded);
    }
}

