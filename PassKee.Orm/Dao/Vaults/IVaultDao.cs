using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PassKee.Orm.Dao.Common;
using PassKee.Orm.Entities.Vaults;

namespace PassKee.Orm.Dao.Vaults;

public interface IVaultDao : IBaseDao
{
    Task<VaultEntity?> GetById(Guid id, CancellationToken cancellationToken = default);
    Task<IList<VaultEntity>> GetByUserId(Guid userId, CancellationToken cancellationToken = default);
    Task<VaultEntity> CreateAsync(Guid userId, string name, byte[] encryptedVaultKey, CancellationToken cancellationToken = default);
}

