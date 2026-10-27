using System;
using System.Threading;
using System.Threading.Tasks;
using PassKee.Business.Services.Storage;
using PassKee.Orm.Dao.Vaults;
using PassKee.Orm.Entities.Vaults;

namespace PassKee.Business.Services.Vaults;

public class VaultManagerService : IVaultManagerService
{
    private readonly IVaultDao _vaultDao;
    private readonly ICredentialDao _credentialDao;
    private readonly IDirectoryDao _directoryDao;
    private readonly ITagDao _tagDao;
    private readonly IFileStorageService _fileStorageService;

    public VaultManagerService(
        IVaultDao vaultDao,
        ICredentialDao credentialDao,
        IDirectoryDao directoryDao,
        ITagDao tagDao,
        IFileStorageService fileStorageService)
    {
        _vaultDao = vaultDao;
        _credentialDao = credentialDao;
        _directoryDao = directoryDao;
        _tagDao = tagDao;
        _fileStorageService = fileStorageService;
    }

    public async Task DeleteVaultAsync(VaultEntity vault, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(vault);

        await _fileStorageService.DeleteVaultFilesAsync(vault.Id, cancellationToken);
        await _credentialDao.DeleteByVaultIdAsync(vault.Id, cancellationToken);
        await _directoryDao.DeleteByVaultIdAsync(vault.Id, cancellationToken);
        await _tagDao.DeleteByVaultIdAsync(vault.Id, cancellationToken);

        await _vaultDao.DeleteAsync(vault, cancellationToken);
    }

    public async Task DeleteVaultAsync(Guid vaultId, CancellationToken cancellationToken = default)
    {
        var vault = await _vaultDao.GetById(vaultId, cancellationToken);
        if (vault != null)
        {
            await DeleteVaultAsync(vault, cancellationToken);
        }
    }
}

