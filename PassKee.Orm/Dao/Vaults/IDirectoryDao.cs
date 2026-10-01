using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PassKee.Orm.Dao.Common;
using PassKee.Orm.Entities.Vaults;

namespace PassKee.Orm.Dao.Vaults;

public interface IDirectoryDao : IBaseDao
{
    Task<DirectoryEntity?> GetById(Guid id, CancellationToken cancellationToken = default);
    Task<IList<DirectoryEntity>> GetByVaultId(Guid vaultId, CancellationToken cancellationToken = default);
    Task<DirectoryEntity> CreateAsync(Guid vaultId, Guid? parentDirectoryId, byte[] encryptedName, CancellationToken cancellationToken = default);
    Task<DirectoryEntity> UpdateAsync(DirectoryEntity directory, Guid? parentDirectoryId, byte[] encryptedName, CancellationToken cancellationToken = default);
    Task DeleteWithDescendantsAsync(DirectoryEntity directory, CancellationToken cancellationToken = default);
}

