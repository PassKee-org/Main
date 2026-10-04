using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PassKee.Orm.Dao.Common;
using PassKee.Orm.Entities.Vaults;

namespace PassKee.Orm.Dao.Vaults;

public interface ITagDao : IBaseDao
{
    Task<TagEntity?> GetById(Guid id, CancellationToken cancellationToken = default);
    Task<IList<TagEntity>> GetByVaultId(Guid vaultId, CancellationToken cancellationToken = default);
    Task<TagEntity> CreateAsync(Guid vaultId, byte[] encryptedName, CancellationToken cancellationToken = default);
    Task<TagEntity> UpdateAsync(TagEntity tag, byte[] encryptedName, CancellationToken cancellationToken = default);
}
