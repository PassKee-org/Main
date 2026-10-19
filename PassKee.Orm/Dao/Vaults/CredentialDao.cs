using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using NHibernate.Linq;
using PassKee.Business.Common.Constants;
using PassKee.Orm.Dao.Common;
using PassKee.Orm.Entities.Vaults;

namespace PassKee.Orm.Dao.Vaults;

public class CredentialDao : BaseDao, ICredentialDao
{
    public CredentialDao(ILifetimeScope scope) : base(scope)
    {
    }

    public async Task<CredentialEntity?> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        return await Session.Query<CredentialEntity>()
            .Where(x => x.Id == id && x.DeletedAt == null)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IList<CredentialEntity>> GetByVaultId(Guid vaultId, CancellationToken cancellationToken = default)
    {
        return await Session.Query<CredentialEntity>()
            .Where(x => x.VaultId == vaultId && x.DeletedAt == null && x.ArchivedAt == null)
            .OrderBy(x => x.Order)
            .ThenBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IList<CredentialEntity>> GetByDirectoryIds(Guid vaultId, ICollection<Guid> directoryIds, CancellationToken cancellationToken = default)
    {
        return await Session.Query<CredentialEntity>()
            .Where(c => c.VaultId == vaultId && c.DirectoryId.HasValue && directoryIds.Contains(c.DirectoryId.Value) && c.DeletedAt == null && c.ArchivedAt == null)
            .OrderBy(c => c.Order)
            .ThenBy(c => c.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IList<CredentialEntity>> GetArchivedByVaultId(Guid vaultId, CancellationToken cancellationToken = default)
    {
        return await Session.Query<CredentialEntity>()
            .Where(x => x.VaultId == vaultId && x.DeletedAt == null && x.ArchivedAt != null)
            .OrderBy(x => x.Order)
            .ThenBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<CredentialEntity> CreateAsync(Guid vaultId, Guid? directoryId, CredentialType type, byte[] encryptedBody, CancellationToken cancellationToken = default)
    {
        var nextOrder = await GetNextOrderAsync(vaultId, directoryId, cancellationToken);
        var cred = new CredentialEntity
        {
            VaultId = vaultId,
            DirectoryId = directoryId,
            Type = type,
            EncryptedBody = encryptedBody,
            Order = nextOrder
        };
        await Session.SaveAsync(cred, cancellationToken);
        return cred;
    }

    public async Task<CredentialEntity> UpdateAsync(CredentialEntity credential, Guid? directoryId, CredentialType type, byte[] encryptedBody, CancellationToken cancellationToken = default)
    {
        if (credential.DirectoryId != directoryId)
        {
            credential.Order = await GetNextOrderAsync(credential.VaultId, directoryId, cancellationToken);
        }

        credential.DirectoryId = directoryId;
        credential.Type = type;
        credential.EncryptedBody = encryptedBody;
        credential.UpdatedAt = DateTime.UtcNow;

        await Session.UpdateAsync(credential, cancellationToken);
        return credential;
    }

    public async Task<CredentialEntity> ArchiveAsync(CredentialEntity credential, CancellationToken cancellationToken = default)
    {
        credential.ArchivedAt = DateTime.UtcNow;
        credential.UpdatedAt = DateTime.UtcNow;

        await Session.UpdateAsync(credential, cancellationToken);
        return credential;
    }

    public async Task UpdatePositionsAsync(Guid vaultId, IDictionary<Guid, int> items, CancellationToken cancellationToken = default)
    {
        if (items.Count == 0)
        {
            return;
        }

        var itemIds = items.Keys.ToList();
        var credentials = await Session.Query<CredentialEntity>()
            .Where(x => x.VaultId == vaultId && itemIds.Contains(x.Id) && x.DeletedAt == null)
            .ToListAsync(cancellationToken);

        foreach (var cred in credentials)
        {
            if (items.TryGetValue(cred.Id, out var newOrder))
            {
                cred.Order = newOrder;
                cred.UpdatedAt = DateTime.UtcNow;
                await Session.UpdateAsync(cred, cancellationToken);
            }
        }
    }

    private async Task<int> GetNextOrderAsync(Guid vaultId, Guid? directoryId, CancellationToken cancellationToken)
    {
        var maxOrder = await Session.Query<CredentialEntity>()
            .Where(x => x.VaultId == vaultId && x.DirectoryId == directoryId && x.DeletedAt == null && x.ArchivedAt == null)
            .Select(x => (int?)x.Order)
            .MaxAsync(cancellationToken);

        return (maxOrder ?? -1) + 1;
    }
}

