using System;
using System.Collections.Generic;
using System.Text;
using PassKee.Api.Shared.Models.Vaults;
using PassKee.Api.Shared.Models.Vaults.Payloads;
using PassKee.Business.Common.Constants;
using PassKee.Business.Common.Helpers;
using PassKee.Business.Common.Utils;
using PassKee.Web.Models.Vaults;

namespace PassKee.Web.Core.Services.Vaults;

public class VaultCryptoService : IVaultCryptoService
{
    public byte[] EncryptVaultKey(byte[] vaultKey, byte[] userPublicKey)
    {
        ArgumentNullException.ThrowIfNull(vaultKey);
        ArgumentNullException.ThrowIfNull(userPublicKey);

        return CryptoUtils.EccEncrypt(userPublicKey, vaultKey);
    }

    public byte[] DecryptVaultKey(byte[] encryptedVaultKey, byte[] userPrivateKey)
    {
        ArgumentNullException.ThrowIfNull(encryptedVaultKey);
        ArgumentNullException.ThrowIfNull(userPrivateKey);

        return CryptoUtils.EccDecrypt(userPrivateKey, encryptedVaultKey);
    }

    public byte[] EncryptDirectoryName(string name, byte[] vaultKey)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(vaultKey);

        return CryptoUtils.AesGcmEncrypt(vaultKey, Encoding.UTF8.GetBytes(name));
    }

    public string DecryptDirectoryName(byte[] encryptedName, byte[] vaultKey)
    {
        ArgumentNullException.ThrowIfNull(encryptedName);
        ArgumentNullException.ThrowIfNull(vaultKey);

        var decryptedBytes = CryptoUtils.AesGcmDecrypt(vaultKey, encryptedName);
        return Encoding.UTF8.GetString(decryptedBytes);
    }

    public List<DecryptedDirectory> DecryptDirectories(IEnumerable<DirectoryDto> directories, byte[] vaultKey)
    {
        ArgumentNullException.ThrowIfNull(directories);
        ArgumentNullException.ThrowIfNull(vaultKey);

        var list = new List<DecryptedDirectory>();
        foreach (var dir in directories)
        {
            var name = DecryptDirectoryName(dir.EncryptedName, vaultKey);
            list.Add(new DecryptedDirectory(dir.Id, dir.VaultId, dir.ParentDirectoryId, name));
        }

        return list;
    }

    public byte[] EncryptTagName(string name, byte[] vaultKey)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(vaultKey);

        return CryptoUtils.AesGcmEncrypt(vaultKey, Encoding.UTF8.GetBytes(name));
    }

    public string DecryptTagName(byte[] encryptedName, byte[] vaultKey)
    {
        ArgumentNullException.ThrowIfNull(encryptedName);
        ArgumentNullException.ThrowIfNull(vaultKey);

        var decryptedBytes = CryptoUtils.AesGcmDecrypt(vaultKey, encryptedName);
        return Encoding.UTF8.GetString(decryptedBytes);
    }

    public List<DecryptedTag> DecryptTags(IEnumerable<TagDto> tags, byte[] vaultKey)
    {
        ArgumentNullException.ThrowIfNull(tags);
        ArgumentNullException.ThrowIfNull(vaultKey);

        var list = new List<DecryptedTag>();
        foreach (var tag in tags)
        {
            var name = DecryptTagName(tag.EncryptedName, vaultKey);
            list.Add(new DecryptedTag(tag.Id, tag.VaultId, name));
        }

        return list;
    }

    public byte[] EncryptCredentialPayload(BaseCredentialPayload payload, byte[] vaultKey)
    {
        ArgumentNullException.ThrowIfNull(payload);
        ArgumentNullException.ThrowIfNull(vaultKey);

        var json = JsonHelper.SerializeToString(payload);
        return CryptoUtils.AesGcmEncrypt(vaultKey, Encoding.UTF8.GetBytes(json));
    }

    public BaseCredentialPayload DecryptCredentialPayload(byte[] encryptedBody, CredentialType type, byte[] vaultKey)
    {
        ArgumentNullException.ThrowIfNull(encryptedBody);
        ArgumentNullException.ThrowIfNull(vaultKey);

        var decryptedBytes = CryptoUtils.AesGcmDecrypt(vaultKey, encryptedBody);
        var json = Encoding.UTF8.GetString(decryptedBytes);

        return type switch
        {
            CredentialType.Login => JsonHelper.DeserializeObject<LoginCredentialPayload>(json) ?? new LoginCredentialPayload(),
            CredentialType.Password => JsonHelper.DeserializeObject<PasswordCredentialPayload>(json) ?? new PasswordCredentialPayload(),
            CredentialType.SecureNote => JsonHelper.DeserializeObject<SecureNoteCredentialPayload>(json) ?? new SecureNoteCredentialPayload(),
            CredentialType.Card => JsonHelper.DeserializeObject<CardCredentialPayload>(json) ?? new CardCredentialPayload(),
            CredentialType.File => JsonHelper.DeserializeObject<FileCredentialPayload>(json) ?? new FileCredentialPayload(),
            CredentialType.Database => JsonHelper.DeserializeObject<DatabaseCredentialPayload>(json) ?? new DatabaseCredentialPayload(),
            CredentialType.SshKey => JsonHelper.DeserializeObject<SshKeyCredentialPayload>(json) ?? new SshKeyCredentialPayload(),
            CredentialType.Server => JsonHelper.DeserializeObject<ServerCredentialPayload>(json) ?? new ServerCredentialPayload(),
            _ => new LoginCredentialPayload { Title = "Unknown Type" }
        };
    }

    public List<DecryptedCredential> DecryptCredentials(IEnumerable<CredentialDto> credentials, byte[] vaultKey)
    {
        ArgumentNullException.ThrowIfNull(credentials);
        ArgumentNullException.ThrowIfNull(vaultKey);

        var list = new List<DecryptedCredential>();
        foreach (var cred in credentials)
        {
            var payload = DecryptCredentialPayload(cred.EncryptedBody, cred.Type, vaultKey);
            list.Add(new DecryptedCredential(cred.Id, cred.VaultId, cred.DirectoryId, cred.Type, payload));
        }

        return list;
    }

    public byte[] EncryptFile(byte[] fileData, byte[] vaultKey)
    {
        ArgumentNullException.ThrowIfNull(fileData);
        ArgumentNullException.ThrowIfNull(vaultKey);

        return CryptoUtils.AesGcmEncrypt(vaultKey, fileData);
    }

    public byte[] DecryptFile(byte[] encryptedData, byte[] vaultKey)
    {
        ArgumentNullException.ThrowIfNull(encryptedData);
        ArgumentNullException.ThrowIfNull(vaultKey);

        return CryptoUtils.AesGcmDecrypt(vaultKey, encryptedData);
    }
}
