using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PassKee.Api.Shared.Models.Vaults;
using PassKee.Api.Shared.Models.Vaults.Payloads;
using PassKee.Business.Common.Constants;
using PassKee.Web.Core.Services.Vaults;
using PassKee.Web.Models.Vaults;
using PassKee.Web.Services.Http;
using PassKee.Business.Common.Utils;

namespace PassKee.Web.Services.Vaults;

public class VaultClientService : IVaultClientService
{
    private readonly ApiService _apiService;
    private readonly IVaultCryptoService _vaultCrypto;

    public VaultClientService(ApiService apiService, IVaultCryptoService vaultCrypto)
    {
        _apiService = apiService;
        _vaultCrypto = vaultCrypto;
    }

    public async Task<List<VaultDto>> GetVaultsAsync()
    {
        var vaults = await _apiService.GetVaultsAsync();
        return vaults ?? new List<VaultDto>();
    }

    public async Task<VaultDto?> CreateVaultAsync(string name, byte[] userPublicKey)
    {
        var vaultKey = CryptoUtils.GenerateRandomBytes(32);
        var encryptedVaultKey = _vaultCrypto.EncryptVaultKey(vaultKey, userPublicKey);
        var request = new CreateVaultRequest
        {
            Name = name,
            EncryptedVaultKey = encryptedVaultKey
        };

        return await _apiService.CreateVaultAsync(request);
    }

    public async Task<VaultDetailsResult> GetVaultDetailsAsync(Guid vaultId, byte[] userPrivateKey)
    {
        var response = await _apiService.GetVaultDetailsAsync(vaultId);
        if (response == null)
        {
            throw new InvalidOperationException($"Failed to load vault details for {vaultId}");
        }

        if (response.Vault?.EncryptedVaultKey == null || response.Vault.EncryptedVaultKey.Length == 0)
        {
            throw new InvalidOperationException("Vault key is missing from vault details.");
        }

        var vaultKey = _vaultCrypto.DecryptVaultKey(response.Vault.EncryptedVaultKey, userPrivateKey);
        var decryptedDirectories = _vaultCrypto.DecryptDirectories(response.Directories, vaultKey);
        var decryptedCredentials = _vaultCrypto.DecryptCredentials(response.Credentials, vaultKey);
        var decryptedTags = _vaultCrypto.DecryptTags(response.Tags, vaultKey);

        return new VaultDetailsResult(vaultKey, decryptedDirectories, decryptedCredentials, decryptedTags);
    }

    public async Task<DirectoryDto?> CreateDirectoryAsync(Guid vaultId, Guid? parentId, string name, byte[] vaultKey)
    {
        var encryptedName = _vaultCrypto.EncryptDirectoryName(name, vaultKey);
        var request = new CreateDirectoryRequest
        {
            VaultId = vaultId,
            ParentDirectoryId = parentId,
            EncryptedName = encryptedName
        };

        return await _apiService.CreateDirectoryAsync(request);
    }

    public async Task<DirectoryDto?> UpdateDirectoryAsync(Guid directoryId, Guid? parentId, string name, byte[] vaultKey)
    {
        var encryptedName = _vaultCrypto.EncryptDirectoryName(name, vaultKey);
        var request = new UpdateDirectoryRequest
        {
            DirectoryId = directoryId,
            ParentDirectoryId = parentId,
            EncryptedName = encryptedName
        };

        return await _apiService.UpdateDirectoryAsync(directoryId, request);
    }

    public Task<DeleteDirectoryResponse?> DeleteDirectoryAsync(Guid directoryId)
    {
        return _apiService.DeleteDirectoryAsync(directoryId);
    }

    public async Task<CredentialDto?> CreateCredentialAsync(Guid vaultId, Guid? directoryId, CredentialType type, BaseCredentialPayload payload, byte[] vaultKey)
    {
        var encryptedBody = _vaultCrypto.EncryptCredentialPayload(payload, vaultKey);
        var request = new CreateCredentialRequest
        {
            VaultId = vaultId,
            DirectoryId = directoryId,
            Type = type,
            EncryptedBody = encryptedBody
        };

        return await _apiService.CreateCredentialAsync(request);
    }

    public async Task<CredentialDto?> UpdateCredentialAsync(Guid credentialId, Guid? directoryId, CredentialType type, BaseCredentialPayload payload, byte[] vaultKey)
    {
        var encryptedBody = _vaultCrypto.EncryptCredentialPayload(payload, vaultKey);
        var request = new UpdateCredentialRequest
        {
            CredentialId = credentialId,
            DirectoryId = directoryId,
            Type = type,
            EncryptedBody = encryptedBody
        };

        return await _apiService.UpdateCredentialAsync(credentialId, request);
    }

    public Task<DeleteCredentialResponse?> DeleteCredentialAsync(Guid credentialId)
    {
        return _apiService.DeleteCredentialAsync(credentialId);
    }

    public async Task<DecryptedTag?> CreateTagAsync(Guid vaultId, string name, byte[] vaultKey)
    {
        var encryptedName = _vaultCrypto.EncryptTagName(name, vaultKey);
        var request = new CreateTagRequest
        {
            VaultId = vaultId,
            EncryptedName = encryptedName
        };

        var dto = await _apiService.CreateTagAsync(request);
        if (dto == null) return null;

        return new DecryptedTag(dto.Id, dto.VaultId, name);
    }

    public async Task<DecryptedTag?> UpdateTagAsync(Guid tagId, string name, byte[] vaultKey)
    {
        var encryptedName = _vaultCrypto.EncryptTagName(name, vaultKey);
        var request = new UpdateTagRequest
        {
            TagId = tagId,
            EncryptedName = encryptedName
        };

        var dto = await _apiService.UpdateTagAsync(tagId, request);
        if (dto == null) return null;

        return new DecryptedTag(dto.Id, dto.VaultId, name);
    }

    public async Task<PassKee.Api.Shared.Models.Storage.StoredFileDto?> UploadFileAsync(Guid vaultId, byte[] rawFileBytes, string originalFileName, byte[] vaultKey)
    {
        var encryptedBytes = _vaultCrypto.EncryptFile(rawFileBytes, vaultKey);
        return await _apiService.UploadVaultFileAsync(vaultId, encryptedBytes, originalFileName);
    }

    public async Task<byte[]?> DownloadFileAsync(Guid fileId, byte[] vaultKey)
    {
        var encryptedBytes = await _apiService.DownloadFileAsync(fileId);
        if (encryptedBytes == null) return null;

        return _vaultCrypto.DecryptFile(encryptedBytes, vaultKey);
    }

    public Task<bool> DeleteFileAsync(Guid fileId)
    {
        return _apiService.DeleteFileAsync(fileId);
    }
}
