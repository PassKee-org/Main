using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PassKee.Business.Common.Constants;
using PassKee.Orm.Dao.Common;
using PassKee.Orm.Entities.Vaults;

namespace PassKee.Orm.Dao.Vaults;

public interface ICredentialDao : IBaseDao
{
    Task<CredentialEntity?> GetById(Guid id, CancellationToken cancellationToken = default);
    Task<IList<CredentialEntity>> GetByVaultId(Guid vaultId, CancellationToken cancellationToken = default);
    Task<IList<CredentialEntity>> GetByDirectoryIds(Guid vaultId, ICollection<Guid> directoryIds, CancellationToken cancellationToken = default);
    Task<CredentialEntity> CreateAsync(Guid vaultId, Guid? directoryId, CredentialType type, byte[] encryptedBody, CancellationToken cancellationToken = default);
    Task<CredentialEntity> UpdateAsync(CredentialEntity credential, Guid? directoryId, CredentialType type, byte[] encryptedBody, CancellationToken cancellationToken = default);
}

