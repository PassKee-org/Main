using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PassKee.Api.Shared.Constants;
using PassKee.Api.Shared.Models.Vaults;

namespace PassKee.Web.Services.Http;

public partial class ApiService
{
    public async Task<List<VaultDto>?> GetVaultsAsync()
    {
        var res = await GetAsync<VaultsResponse>(ApiUrl.Vaults);
        return res?.Vaults;
    }

    public async Task<VaultDetailsResponse?> GetVaultDetailsAsync(Guid vaultId)
    {
        return await GetAsync<VaultDetailsResponse>(ApiUrl.VaultDetails(vaultId));
    }

    public async Task<VaultDto?> CreateVaultAsync(CreateVaultRequest request)
    {
        var res = await PostAsync<VaultResponse>(ApiUrl.Vaults, request);
        return res?.Vault;
    }

    public async Task<DirectoryDto?> CreateDirectoryAsync(CreateDirectoryRequest request)
    {
        var res = await PostAsync<DirectoryResponse>(ApiUrl.VaultDirectories, request);
        return res?.Directory;
    }

    public async Task<DirectoryDto?> UpdateDirectoryAsync(Guid directoryId, UpdateDirectoryRequest request)
    {
        var res = await PutAsync<DirectoryResponse>(ApiUrl.VaultDirectory(directoryId), request);
        return res?.Directory;
    }

    public async Task<bool> DeleteDirectoryAsync(Guid directoryId)
    {
        var res = await DeleteAsync<ActionResponse>(ApiUrl.VaultDirectory(directoryId));
        return res?.Success ?? false;
    }

    public async Task<CredentialDto?> CreateCredentialAsync(CreateCredentialRequest request)
    {
        var res = await PostAsync<CredentialResponse>(ApiUrl.VaultCredentials, request);
        return res?.Credential;
    }

    public async Task<CredentialDto?> UpdateCredentialAsync(Guid credentialId, UpdateCredentialRequest request)
    {
        var res = await PutAsync<CredentialResponse>(ApiUrl.VaultCredential(credentialId), request);
        return res?.Credential;
    }

    public async Task<bool> DeleteCredentialAsync(Guid credentialId)
    {
        var res = await DeleteAsync<ActionResponse>(ApiUrl.VaultCredential(credentialId));
        return res?.Success ?? false;
    }
}
