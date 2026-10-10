using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PassKee.Api.Shared.Models.Vaults;
using PassKee.Api.Shared.Models.Vaults.Payloads;
using PassKee.Business.Common.Constants;
using PassKee.Web.Models.Vaults;

namespace PassKee.Web.Services.Vaults;

public record VaultDetailsResult(
    byte[] ActiveVaultKey,
    List<DecryptedDirectory> Directories,
    List<DecryptedCredential> Credentials,
    List<DecryptedTag> Tags
);

public interface IVaultClientService
{
    Task<List<VaultDto>> GetVaultsAsync();
    Task<VaultDto?> CreateVaultAsync(string name, byte[] userPublicKey);
    Task<VaultDetailsResult> GetVaultDetailsAsync(Guid vaultId, byte[] userPrivateKey);
    Task<DirectoryDto?> CreateDirectoryAsync(Guid vaultId, Guid? parentId, string name, byte[] vaultKey);
    Task<DirectoryDto?> UpdateDirectoryAsync(Guid directoryId, Guid? parentId, string name, byte[] vaultKey);
    Task<DeleteDirectoryResponse?> DeleteDirectoryAsync(Guid directoryId);
    Task<CredentialDto?> CreateCredentialAsync(Guid vaultId, Guid? directoryId, CredentialType type, BaseCredentialPayload payload, byte[] vaultKey);
    Task<CredentialDto?> UpdateCredentialAsync(Guid credentialId, Guid? directoryId, CredentialType type, BaseCredentialPayload payload, byte[] vaultKey);
    Task<DeleteCredentialResponse?> DeleteCredentialAsync(Guid credentialId);
    Task<CredentialDto?> ArchiveCredentialAsync(Guid credentialId);
    Task<List<DecryptedCredential>> GetArchivedCredentialsAsync(Guid vaultId, byte[] vaultKey);
    Task<DecryptedTag?> CreateTagAsync(Guid vaultId, string name, byte[] vaultKey);
    Task<DecryptedTag?> UpdateTagAsync(Guid tagId, string name, byte[] vaultKey);
    Task<PassKee.Api.Shared.Models.Storage.StoredFileDto?> UploadFileAsync(Guid vaultId, byte[] rawFileBytes, string originalFileName, byte[] vaultKey);
    Task<byte[]?> DownloadFileAsync(Guid fileId, byte[] vaultKey);
    Task<bool> DeleteFileAsync(Guid fileId);
}
