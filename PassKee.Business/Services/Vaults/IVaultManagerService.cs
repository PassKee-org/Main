using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.Abstractions;
using PassKee.Orm.Entities.Vaults;

namespace PassKee.Business.Services.Vaults;

public interface IVaultManagerService : IDomainService
{
    Task DeleteVaultAsync(VaultEntity vault, CancellationToken cancellationToken = default);
    Task DeleteVaultAsync(Guid vaultId, CancellationToken cancellationToken = default);
}

