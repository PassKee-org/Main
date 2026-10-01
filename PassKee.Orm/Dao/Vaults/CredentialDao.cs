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
            .Where(x => x.VaultId == vaultId && x.DeletedAt == null)
            .ToListAsync(cancellationToken);
    }

    public async Task<IList<CredentialEntity>> GetByDirectoryIds(Guid vaultId, ICollection<Guid> directoryIds, CancellationToken cancellationToken = default)
    {
        return await Session.Query<CredentialEntity>()
            .Where(c => c.VaultId == vaultId && c.DirectoryId.HasValue && directoryIds.Contains(c.DirectoryId.Value) && c.DeletedAt == null)
            .ToListAsync(cancellationToken);
    }

    public async Task<CredentialEntity> CreateAsync(Guid vaultId, Guid? directoryId, CredentialType type, byte[] encryptedBody, CancellationToken cancellationToken = default)
    {
        var cred = new CredentialEntity
        {
            VaultId = vaultId,
            DirectoryId = directoryId,
            Type = type,
            EncryptedBody = encryptedBody
        };
        await Session.SaveAsync(cred, cancellationToken);
        return cred;
    }

    public async Task<CredentialEntity> UpdateAsync(CredentialEntity credential, Guid? directoryId, CredentialType type, byte[] encryptedBody, CancellationToken cancellationToken = default)
    {
        credential.DirectoryId = directoryId;
        credential.Type = type;
        credential.EncryptedBody = encryptedBody;
        credential.UpdatedAt = DateTime.UtcNow;

        await Session.UpdateAsync(credential, cancellationToken);
        return credential;
    }
}

