using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PassKee.Api.Shared.Models.Vaults;
using PassKee.Api.Shared.Models.Vaults.Enums;
using PassKee.Api.Shared.Models.Vaults.Payloads;
using PassKee.Web.Models.Vaults;

namespace PassKee.Web.Services.Vaults;

public record VaultDetailsResult(
    byte[] ActiveVaultKey,
    List<DecryptedDirectory> Directories,
    List<DecryptedCredential> Credentials
);

public interface IVaultClientService
{
    Task<List<VaultDto>> GetVaultsAsync();
    Task<VaultDto?> CreateVaultAsync(string name, byte[] userPublicKey);
    Task<VaultDetailsResult> GetVaultDetailsAsync(Guid vaultId, byte[] userPrivateKey);
    Task<DirectoryDto?> CreateDirectoryAsync(Guid vaultId, Guid? parentId, string name, byte[] vaultKey);
    Task<DirectoryDto?> UpdateDirectoryAsync(Guid directoryId, Guid? parentId, string name, byte[] vaultKey);
    Task<bool> DeleteDirectoryAsync(Guid directoryId);
    Task<CredentialDto?> CreateCredentialAsync(Guid vaultId, Guid? directoryId, CredentialType type, BaseCredentialPayload payload, byte[] vaultKey);
    Task<CredentialDto?> UpdateCredentialAsync(Guid credentialId, Guid? directoryId, CredentialType type, BaseCredentialPayload payload, byte[] vaultKey);
    Task<bool> DeleteCredentialAsync(Guid credentialId);
}
