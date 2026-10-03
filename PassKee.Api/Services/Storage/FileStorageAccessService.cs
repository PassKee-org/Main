using System;
using System.Threading.Tasks;
using PassKee.Api.Services.Security;
using PassKee.Business.Common.Constants;
using PassKee.Business.Common.Exceptions.Api;
using PassKee.Orm.Dao.Vaults;
using PassKee.Orm.Entities.Storage;

namespace PassKee.Api.Services.Storage;

public class FileStorageAccessService : IFileStorageAccessService
{
    private readonly ISecurityService _securityService;
    private readonly IVaultDao _vaultDao;

    public FileStorageAccessService(ISecurityService securityService, IVaultDao vaultDao)
    {
        _securityService = securityService;
        _vaultDao = vaultDao;
    }

    public async Task CheckAccessAsync(AccessLevel accessLevel, Guid userId, FileStorageEntity file)
    {
        switch (file)
        {
            case VaultFileStorageEntity vaultFile:
                var vault = await _vaultDao.GetById(vaultFile.VaultId)
                            ?? throw new RecordNotFoundException("Vault not found");
                await _securityService.CheckAccess(accessLevel, userId, vault);
                break;
            default:
                throw new NotSupportedException($"Unsupported stored file type: {file.GetType().Name}");
        }
    }
}
