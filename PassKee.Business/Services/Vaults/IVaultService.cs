using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Abstractions;
using PassKee.Orm.Entities.Vaults;

namespace PassKee.Business.Services.Vaults;

public interface IVaultService : IScopedDomainService
{
    Task<IList<VaultEntity>> GetUserVaultsAsync(Guid userId);
    Task<VaultEntity> GetVaultAsync(Guid userId, Guid vaultId);
    Task<VaultEntity> CreateVaultAsync(Guid userId, string name, byte[] encryptedVaultKey);
    Task<IList<DirectoryEntity>> GetVaultDirectoriesAsync(Guid vaultId);
    Task<IList<CredentialEntity>> GetVaultCredentialsAsync(Guid vaultId);

    Task<DirectoryEntity> CreateDirectoryAsync(Guid userId, Guid vaultId, Guid? parentDirectoryId, byte[] encryptedName);
    Task<DirectoryEntity> UpdateDirectoryAsync(Guid userId, Guid directoryId, Guid? parentDirectoryId, byte[] encryptedName);
    Task DeleteDirectoryAsync(Guid userId, Guid directoryId);

    Task<CredentialEntity> CreateCredentialAsync(Guid userId, Guid vaultId, Guid? directoryId, CredentialType type, byte[] encryptedBody);
    Task<CredentialEntity> UpdateCredentialAsync(Guid userId, Guid credentialId, Guid? directoryId, CredentialType type, byte[] encryptedBody);
    Task DeleteCredentialAsync(Guid userId, Guid credentialId);
}
