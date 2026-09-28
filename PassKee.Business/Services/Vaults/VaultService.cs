using Persistence.Transactions.Behaviors;
using PassKee.Orm.Dao;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NHibernate;
using NHibernate.Linq;
using PassKee.Orm.Entities.Vaults;
using PassKee.Business.Common.Exceptions;

namespace PassKee.Business.Services.Vaults;

public class VaultService : IVaultService
{
    private readonly IDbSessionProvider _sessionProvider;

    public VaultService(IDbSessionProvider sessionProvider)
    {
        _sessionProvider = sessionProvider;
    }

    public async Task<IList<VaultEntity>> GetUserVaultsAsync(Guid userId)
    {
        return await _sessionProvider.CurrentSession.Query<VaultEntity>()
            .Where(v => v.UserId == userId)
            .ToListAsync();
    }

    public async Task<VaultEntity> GetVaultAsync(Guid userId, Guid vaultId)
    {
        var vault = await _sessionProvider.CurrentSession.GetAsync<VaultEntity>(vaultId);
        if (vault == null || vault.UserId != userId)
        {
            throw new Exception("Vault not found.");
        }
        return vault;
    }

    public async Task<VaultEntity> CreateVaultAsync(Guid userId, string name, byte[] encryptedVaultKey)
    {
        var vault = new VaultEntity
        {
            UserId = userId,
            Name = name,
            EncryptedVaultKey = encryptedVaultKey
        };
        await _sessionProvider.CurrentSession.SaveAsync(vault);
        return vault;
    }

    public async Task<IList<DirectoryEntity>> GetVaultDirectoriesAsync(Guid vaultId)
    {
        return await _sessionProvider.CurrentSession.Query<DirectoryEntity>()
            .Where(d => d.VaultId == vaultId)
            .ToListAsync();
    }

    public async Task<IList<CredentialEntity>> GetVaultCredentialsAsync(Guid vaultId)
    {
        return await _sessionProvider.CurrentSession.Query<CredentialEntity>()
            .Where(c => c.VaultId == vaultId)
            .ToListAsync();
    }

    private async Task ValidateVaultAccessAsync(Guid userId, Guid vaultId)
    {
        var vault = await _sessionProvider.CurrentSession.GetAsync<VaultEntity>(vaultId);
        if (vault == null || vault.UserId != userId)
            throw DomainException.ItemNotFoundException;
    }

    public async Task<DirectoryEntity> CreateDirectoryAsync(Guid userId, Guid vaultId, Guid? parentDirectoryId, byte[] encryptedName)
    {
        await ValidateVaultAccessAsync(userId, vaultId);

        var dir = new DirectoryEntity
        {
            VaultId = vaultId,
            ParentDirectoryId = parentDirectoryId,
            EncryptedName = encryptedName
        };

        await _sessionProvider.CurrentSession.SaveAsync(dir);
        return dir;
    }

    public async Task<DirectoryEntity> UpdateDirectoryAsync(Guid userId, Guid directoryId, Guid? parentDirectoryId, byte[] encryptedName)
    {
        var dir = await _sessionProvider.CurrentSession.GetAsync<DirectoryEntity>(directoryId) ?? throw DomainException.ItemNotFoundException;
        await ValidateVaultAccessAsync(userId, dir.VaultId);

        dir.ParentDirectoryId = parentDirectoryId;
        dir.EncryptedName = encryptedName;

        await _sessionProvider.CurrentSession.UpdateAsync(dir);
        return dir;
    }

    public async Task DeleteDirectoryAsync(Guid userId, Guid directoryId)
    {
        var dir = await _sessionProvider.CurrentSession.GetAsync<DirectoryEntity>(directoryId) ?? throw DomainException.ItemNotFoundException;
        await ValidateVaultAccessAsync(userId, dir.VaultId);

        await _sessionProvider.CurrentSession.DeleteAsync(dir);
    }

    public async Task<CredentialEntity> CreateCredentialAsync(Guid userId, Guid vaultId, Guid? directoryId, CredentialType type, byte[] encryptedBody)
    {
        await ValidateVaultAccessAsync(userId, vaultId);

        var cred = new CredentialEntity
        {
            VaultId = vaultId,
            DirectoryId = directoryId,
            Type = type,
            EncryptedBody = encryptedBody
        };

        await _sessionProvider.CurrentSession.SaveAsync(cred);
        return cred;
    }

    public async Task<CredentialEntity> UpdateCredentialAsync(Guid userId, Guid credentialId, Guid? directoryId, CredentialType type, byte[] encryptedBody)
    {
        var cred = await _sessionProvider.CurrentSession.GetAsync<CredentialEntity>(credentialId) ?? throw DomainException.ItemNotFoundException;
        await ValidateVaultAccessAsync(userId, cred.VaultId);

        cred.DirectoryId = directoryId;
        cred.Type = type;
        cred.EncryptedBody = encryptedBody;

        await _sessionProvider.CurrentSession.UpdateAsync(cred);
        return cred;
    }

    public async Task DeleteCredentialAsync(Guid userId, Guid credentialId)
    {
        var cred = await _sessionProvider.CurrentSession.GetAsync<CredentialEntity>(credentialId) ?? throw DomainException.ItemNotFoundException;
        await ValidateVaultAccessAsync(userId, cred.VaultId);

        await _sessionProvider.CurrentSession.DeleteAsync(cred);
    }
}
