using System.Threading.Tasks;
using Api.Requests.Abstractions;
using PassKee.Api.Services.Security;
using PassKee.Api.Shared.Models.Vaults;
using PassKee.Business.Common.Constants;
using PassKee.Business.Services.Http;
using PassKee.Orm.Dao.Vaults;

namespace PassKee.Api.Controllers.Vaults.Actions;

public class UpdateCredentialPositionsRequestHandler : IAsyncRequestHandler<UpdateCredentialPositionsRequest>
{
    private readonly IVaultDao _vaultDao;
    private readonly ICredentialDao _credentialDao;
    private readonly ISecurityService _securityService;
    private readonly IApiRequestService _apiRequestService;

    public UpdateCredentialPositionsRequestHandler(
        IVaultDao vaultDao,
        ICredentialDao credentialDao,
        ISecurityService securityService,
        IApiRequestService apiRequestService)
    {
        _vaultDao = vaultDao;
        _credentialDao = credentialDao;
        _securityService = securityService;
        _apiRequestService = apiRequestService;
    }

    public async Task ExecuteAsync(UpdateCredentialPositionsRequest request)
    {
        var userId = _apiRequestService.GetCurrentUserId();
        var vault = await _vaultDao.GetById(request.VaultId);
        await _securityService.CheckAccess(AccessLevel.Write, userId, vault);

        await _credentialDao.UpdatePositionsAsync(vault!.Id, request.Items);
    }
}

