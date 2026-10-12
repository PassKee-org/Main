using System;
using System.Threading;
using System.Threading.Tasks;
using PassKee.Orm.Dao.Common;
using PassKee.Orm.Entities.Vaults;

namespace PassKee.Orm.Dao.Vaults;

public interface ICredentialShareDao : IBaseDao
{
    Task<CredentialShareEntity?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<CredentialShareEntity> CreateAsync(CredentialShareEntity entity, CancellationToken cancellationToken = default);
    Task IncrementViewCountAsync(CredentialShareEntity entity, CancellationToken cancellationToken = default);
    Task DeletePermanentlyAsync(CredentialShareEntity entity, CancellationToken cancellationToken = default);
}

