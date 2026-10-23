using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Api.Requests.Abstractions;
using PassKee.Api.Services.Security;
using PassKee.Api.Shared.Models.Vaults;
using PassKee.Business.Common.Constants;
using PassKee.Business.Services.Http;
using PassKee.Orm.Dao.Vaults;
using PassKee.Orm.Entities.Vaults;

namespace PassKee.Api.Controllers.Vaults.Actions;

public class DeleteCredentialsRequestHandler : IAsyncRequestHandler<DeleteCredentialsRequest, DeleteCredentialsResponse>
{
    private readonly ICredentialDao _credentialDao;
    private readonly ISecurityService _securityService;
    private readonly IApiRequestService _apiRequestService;

    public DeleteCredentialsRequestHandler(
        ICredentialDao credentialDao,
        ISecurityService securityService,
        IApiRequestService apiRequestService)
    {
        _credentialDao = credentialDao;
        _securityService = securityService;
        _apiRequestService = apiRequestService;
    }

    public async Task<DeleteCredentialsResponse> ExecuteAsync(DeleteCredentialsRequest request)
    {
        var userId = _apiRequestService.GetCurrentUserId();
        if (request.CredentialIds == null || request.CredentialIds.Count == 0)
        {
            return new DeleteCredentialsResponse { CredentialIds = [] };
        }

        var credsToDelete = new List<CredentialEntity>();
        foreach (var credId in request.CredentialIds.Distinct())
        {
            var cred = await _credentialDao.GetById(credId);
            await _securityService.CheckAccess(AccessLevel.Write, userId, cred);
            credsToDelete.Add(cred!);
        }

        if (credsToDelete.Count > 0)
        {
            await _credentialDao.DeleteAsync(credsToDelete);
        }

        return new DeleteCredentialsResponse
        {
            CredentialIds = credsToDelete.Select(c => c.Id).ToList()
        };
    }
}

