using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using NHibernate.Linq;
using PassKee.Orm.Dao.Common;
using PassKee.Orm.Entities.Vaults;

namespace PassKee.Orm.Dao.Vaults;

public class CredentialShareDao : BaseDao, ICredentialShareDao
{
    public CredentialShareDao(ILifetimeScope scope) : base(scope)
    {
    }

    public async Task<CredentialShareEntity?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return await Session.Query<CredentialShareEntity>()
            .Where(x => x.Token == token && x.DeletedAt == null)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<CredentialShareEntity> CreateAsync(CredentialShareEntity entity, CancellationToken cancellationToken = default)
    {
        await Session.SaveAsync(entity, cancellationToken);
        return entity;
    }

    public async Task IncrementViewCountAsync(CredentialShareEntity entity, CancellationToken cancellationToken = default)
    {
        entity.ViewCount++;
        entity.UpdatedAt = DateTime.UtcNow;
        await Session.UpdateAsync(entity, cancellationToken);
    }

    public async Task DeletePermanentlyAsync(CredentialShareEntity entity, CancellationToken cancellationToken = default)
    {
        await Session.DeleteAsync(entity, cancellationToken);
    }
}

