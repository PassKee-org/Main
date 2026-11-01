using System.Threading.Tasks;
using Api.Requests.Abstractions;
using AutoMapper;
using PassKee.Api.Services.Security;
using PassKee.Api.Shared.Models.Vaults;
using PassKee.Business.Common.Constants;
using PassKee.Business.Common.Exceptions.Api;
using PassKee.Business.Services.Http;
using PassKee.Orm.Dao.Vaults;

namespace PassKee.Api.Controllers.Vaults.Actions;

public class RestoreCredentialRequestHandler : IAsyncRequestHandler<RestoreCredentialRequest, CredentialResponse>
{
    private readonly ICredentialDao _credentialDao;
    private readonly ISecurityService _securityService;
    private readonly IApiRequestService _apiRequestService;
    private readonly IMapper _mapper;

    public RestoreCredentialRequestHandler(
        ICredentialDao credentialDao,
        ISecurityService securityService,
        IApiRequestService apiRequestService,
        IMapper mapper)
    {
        _credentialDao = credentialDao;
        _securityService = securityService;
        _apiRequestService = apiRequestService;
        _mapper = mapper;
    }

    public async Task<CredentialResponse> ExecuteAsync(RestoreCredentialRequest request)
    {
        var userId = _apiRequestService.GetCurrentUserId();
        var cred = await _credentialDao.GetById(request.CredentialId);
        await _securityService.CheckAccess(AccessLevel.Write, userId, cred);

        if (cred!.ArchivedAt == null)
        {
            throw new RecordCanNotBeModifiedException("Credential is not archived.");
        }

        await _credentialDao.RestoreAsync(cred!);
        return new CredentialResponse
        {
            Credential = _mapper.Map<CredentialDto>(cred)
        };
    }
}

