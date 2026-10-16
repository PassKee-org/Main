using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using NHibernate.Linq;
using PassKee.Orm.Dao.Common;
using PassKee.Orm.Entities.Vaults;

namespace PassKee.Orm.Dao.Vaults;

public class DirectoryDao : BaseDao, IDirectoryDao
{
    public DirectoryDao(ILifetimeScope scope) : base(scope)
    {
    }

    public async Task<DirectoryEntity?> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        return await Session.Query<DirectoryEntity>()
            .Where(x => x.Id == id && x.DeletedAt == null)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IList<DirectoryEntity>> GetByVaultId(Guid vaultId, CancellationToken cancellationToken = default)
    {
        return await Session.Query<DirectoryEntity>()
            .Where(x => x.VaultId == vaultId && x.DeletedAt == null)
            .ToListAsync(cancellationToken);
    }

    public async Task<DirectoryEntity> CreateAsync(Guid vaultId, Guid? parentDirectoryId, byte[] encryptedName, CancellationToken cancellationToken = default)
    {
        var dir = new DirectoryEntity
        {
            VaultId = vaultId,
            ParentDirectoryId = parentDirectoryId,
            EncryptedName = encryptedName
        };
        await Session.SaveAsync(dir, cancellationToken);
        return dir;
    }

    public async Task<DirectoryEntity> UpdateAsync(DirectoryEntity directory, Guid? parentDirectoryId, byte[] encryptedName, CancellationToken cancellationToken = default)
    {
        directory.ParentDirectoryId = parentDirectoryId;
        directory.EncryptedName = encryptedName;
        directory.UpdatedAt = DateTime.UtcNow;

        await Session.UpdateAsync(directory, cancellationToken);
        return directory;
    }

    public async Task<DirectoryEntity> MoveAsync(DirectoryEntity directory, Guid? parentDirectoryId, CancellationToken cancellationToken = default)
    {
        directory.ParentDirectoryId = parentDirectoryId;
        directory.UpdatedAt = DateTime.UtcNow;

        await Session.UpdateAsync(directory, cancellationToken);
        return directory;
    }

    public async Task<(List<Guid> DeletedDirectoryIds, List<Guid> ArchivedCredentialIds)> DeleteWithDescendantsAsync(DirectoryEntity directory, CancellationToken cancellationToken = default)
    {
        var allVaultDirs = await GetByVaultId(directory.VaultId, cancellationToken);

        var lookup = allVaultDirs
            .Where(d => d.ParentDirectoryId.HasValue)
            .ToLookup(d => d.ParentDirectoryId!.Value);

        var visited = new HashSet<Guid> { directory.Id };
        var dirsToDelete = new List<DirectoryEntity>();
        void CollectDescendants(Guid parentId)
        {
            foreach (var child in lookup[parentId])
            {
                if (visited.Add(child.Id))
                {
                    CollectDescendants(child.Id);
                    dirsToDelete.Add(child);
                }
            }
        }

        CollectDescendants(directory.Id);
        dirsToDelete.Add(directory);

        var dirIds = dirsToDelete.Select(d => d.Id).ToList();
        var dirIdsNullable = dirIds.Cast<Guid?>().ToList();

        var credentialsToArchive = await Session.Query<CredentialEntity>()
            .Where(c => c.VaultId == directory.VaultId && dirIdsNullable.Contains(c.DirectoryId) && c.DeletedAt == null && c.ArchivedAt == null)
            .ToListAsync(cancellationToken);

        var now = DateTime.UtcNow;
        foreach (var credential in credentialsToArchive)
        {
            credential.ArchivedAt = now;
            credential.UpdatedAt = now;
            await Session.UpdateAsync(credential, cancellationToken);
        }

        var credIds = credentialsToArchive.Select(c => c.Id).ToList();

        await DeleteAsync(dirsToDelete, cancellationToken);

        return (dirIds, credIds);
    }
}

