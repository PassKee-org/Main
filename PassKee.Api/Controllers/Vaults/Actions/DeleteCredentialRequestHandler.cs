using System.Threading.Tasks;
using Api.Requests.Abstractions;
using PassKee.Api.Services.Security;
using PassKee.Api.Shared.Models.Vaults;
using PassKee.Business.Common.Constants;
using PassKee.Business.Services.Http;
using PassKee.Orm.Dao.Vaults;

namespace PassKee.Api.Controllers.Vaults.Actions;

public class DeleteCredentialRequestHandler : IAsyncRequestHandler<DeleteCredentialRequest, DeleteCredentialResponse>
{
    private readonly ICredentialDao _credentialDao;
    private readonly ISecurityService _securityService;
    private readonly IApiRequestService _apiRequestService;

    public DeleteCredentialRequestHandler(
        ICredentialDao credentialDao,
        ISecurityService securityService,
        IApiRequestService apiRequestService)
    {
        _credentialDao = credentialDao;
        _securityService = securityService;
        _apiRequestService = apiRequestService;
    }

    public async Task<DeleteCredentialResponse> ExecuteAsync(DeleteCredentialRequest request)
    {
        var userId = _apiRequestService.GetCurrentUserId();
        var cred = await _credentialDao.GetById(request.CredentialId);
        await _securityService.CheckAccess(AccessLevel.Write, userId, cred);

        await _credentialDao.DeleteAsync(cred!);
        return new DeleteCredentialResponse
        {
            Success = true,
            CredentialId = request.CredentialId
        };
    }
}
