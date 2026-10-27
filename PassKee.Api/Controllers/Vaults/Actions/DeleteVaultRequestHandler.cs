using System.Threading.Tasks;
using Api.Requests.Abstractions;
using PassKee.Api.Services.Security;
using PassKee.Api.Shared.Models.Vaults;
using PassKee.Business.Common.Constants;
using PassKee.Business.Services.Http;
using PassKee.Business.Services.Vaults;
using PassKee.Orm.Dao.Vaults;

namespace PassKee.Api.Controllers.Vaults.Actions;

public class DeleteVaultRequestHandler : IAsyncRequestHandler<DeleteVaultRequest, DeleteVaultResponse>
{
    private readonly IVaultDao _vaultDao;
    private readonly IVaultManagerService _vaultManagerService;
    private readonly ISecurityService _securityService;
    private readonly IApiRequestService _apiRequestService;

    public DeleteVaultRequestHandler(
        IVaultDao vaultDao,
        IVaultManagerService vaultManagerService,
        ISecurityService securityService,
        IApiRequestService apiRequestService)
    {
        _vaultDao = vaultDao;
        _vaultManagerService = vaultManagerService;
        _securityService = securityService;
        _apiRequestService = apiRequestService;
    }

    public async Task<DeleteVaultResponse> ExecuteAsync(DeleteVaultRequest request)
    {
        var userId = _apiRequestService.GetCurrentUserId();
        var vault = await _vaultDao.GetById(request.VaultId);
        await _securityService.CheckAccess(AccessLevel.Write, userId, vault);

        await _vaultManagerService.DeleteVaultAsync(vault!);

        return new DeleteVaultResponse
        {
            VaultId = vault!.Id
        };
    }
}
