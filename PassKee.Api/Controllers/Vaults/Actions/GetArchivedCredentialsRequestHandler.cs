using System.Collections.Generic;
using System.Threading.Tasks;
using Api.Requests.Abstractions;
using AutoMapper;
using PassKee.Api.Services.Security;
using PassKee.Api.Shared.Models.Vaults;
using PassKee.Business.Common.Constants;
using PassKee.Business.Services.Http;
using PassKee.Orm.Dao.Vaults;

namespace PassKee.Api.Controllers.Vaults.Actions;

public class GetArchivedCredentialsRequestHandler : IAsyncRequestHandler<GetArchivedCredentialsRequest, ArchivedCredentialsResponse>
{
    private readonly IVaultDao _vaultDao;
    private readonly ICredentialDao _credentialDao;
    private readonly ISecurityService _securityService;
    private readonly IApiRequestService _apiRequestService;
    private readonly IMapper _mapper;

    public GetArchivedCredentialsRequestHandler(
        IVaultDao vaultDao,
        ICredentialDao credentialDao,
        ISecurityService securityService,
        IApiRequestService apiRequestService,
        IMapper mapper)
    {
        _vaultDao = vaultDao;
        _credentialDao = credentialDao;
        _securityService = securityService;
        _apiRequestService = apiRequestService;
        _mapper = mapper;
    }

    public async Task<ArchivedCredentialsResponse> ExecuteAsync(GetArchivedCredentialsRequest request)
    {
        var userId = _apiRequestService.GetCurrentUserId();
        var vault = await _vaultDao.GetById(request.VaultId);
        await _securityService.CheckAccess(AccessLevel.Read, userId, vault);

        var credentials = await _credentialDao.GetArchivedByVaultId(request.VaultId);

        return new ArchivedCredentialsResponse
        {
            Credentials = _mapper.Map<List<CredentialDto>>(credentials)
        };
    }
}

