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

public class VaultDao : BaseDao, IVaultDao
{
    public VaultDao(ILifetimeScope scope) : base(scope)
    {
    }

    public async Task<VaultEntity?> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        return await Session.Query<VaultEntity>()
            .Where(x => x.Id == id && x.DeletedAt == null)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IList<VaultEntity>> GetByUserId(Guid userId, CancellationToken cancellationToken = default)
    {
        return await Session.Query<VaultEntity>()
            .Where(x => x.UserId == userId && x.DeletedAt == null)
            .ToListAsync(cancellationToken);
    }

    public async Task<VaultEntity> CreateAsync(Guid userId, string name, byte[] encryptedVaultKey, CancellationToken cancellationToken = default)
    {
        var vault = new VaultEntity
        {
            UserId = userId,
            Name = name,
            EncryptedVaultKey = encryptedVaultKey
        };
        await Session.SaveAsync(vault, cancellationToken);
        return vault;
    }
}

