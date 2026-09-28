using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PassKee.Api.Shared.Models.Vaults;

namespace PassKee.Web.Services.Http;

public partial class ApiService
{
    public async Task<List<VaultDto>?> GetVaultsAsync()
    {
        var res = await GetAsync<VaultsResponse>("api/vaults");
        return res?.Vaults;
    }

    public async Task<VaultDetailsResponse?> GetVaultDetailsAsync(Guid vaultId)
    {
        return await GetAsync<VaultDetailsResponse>($"api/vaults/{vaultId}");
    }

    public async Task<VaultDto?> CreateVaultAsync(CreateVaultRequest request)
    {
        var res = await PostAsync<VaultResponse>("api/vaults", request);
        return res?.Vault;
    }

    public async Task<DirectoryDto?> CreateDirectoryAsync(CreateDirectoryRequest request)
    {
        var res = await PostAsync<DirectoryResponse>("api/vaults/directories", request);
        return res?.Directory;
    }

    public async Task<DirectoryDto?> UpdateDirectoryAsync(Guid directoryId, UpdateDirectoryRequest request)
    {
        var res = await PutAsync<DirectoryResponse>($"api/vaults/directories/{directoryId}", request);
        return res?.Directory;
    }

    public async Task<bool> DeleteDirectoryAsync(Guid directoryId)
    {
        var res = await DeleteAsync<ActionResponse>($"api/vaults/directories/{directoryId}");
        return res?.Success ?? false;
    }

    public async Task<CredentialDto?> CreateCredentialAsync(CreateCredentialRequest request)
    {
        var res = await PostAsync<CredentialResponse>("api/vaults/credentials", request);
        return res?.Credential;
    }

    public async Task<CredentialDto?> UpdateCredentialAsync(Guid credentialId, UpdateCredentialRequest request)
    {
        var res = await PutAsync<CredentialResponse>($"api/vaults/credentials/{credentialId}", request);
        return res?.Credential;
    }

    public async Task<bool> DeleteCredentialAsync(Guid credentialId)
    {
        var res = await DeleteAsync<ActionResponse>($"api/vaults/credentials/{credentialId}");
        return res?.Success ?? false;
    }
}
