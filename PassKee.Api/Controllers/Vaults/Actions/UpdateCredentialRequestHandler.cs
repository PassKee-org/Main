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

public class UpdateCredentialRequestHandler : IAsyncRequestHandler<UpdateCredentialRequest, CredentialResponse>
{
    private readonly IDirectoryDao _directoryDao;
    private readonly ICredentialDao _credentialDao;
    private readonly ISecurityService _securityService;
    private readonly IApiRequestService _apiRequestService;
    private readonly IMapper _mapper;

    public UpdateCredentialRequestHandler(
        IDirectoryDao directoryDao,
        ICredentialDao credentialDao,
        ISecurityService securityService,
        IApiRequestService apiRequestService,
        IMapper mapper)
    {
        _directoryDao = directoryDao;
        _credentialDao = credentialDao;
        _securityService = securityService;
        _apiRequestService = apiRequestService;
        _mapper = mapper;
    }

    public async Task<CredentialResponse> ExecuteAsync(UpdateCredentialRequest request)
    {
        var userId = _apiRequestService.GetCurrentUserId();
        var cred = await _credentialDao.GetById(request.CredentialId);
        await _securityService.CheckAccess(AccessLevel.Write, userId, cred);

        if (request.DirectoryId.HasValue)
        {
            var dir = await _directoryDao.GetById(request.DirectoryId.Value);
            await _securityService.CheckAccess(AccessLevel.Write, userId, dir);
            if (dir!.VaultId != cred!.VaultId)
            {
                throw new HasNoAccessException();
            }
        }

        var updated = await _credentialDao.UpdateAsync(cred!, request.DirectoryId, request.Type, request.EncryptedBody);
        return new CredentialResponse { Credential = _mapper.Map<CredentialDto>(updated) };
    }
}
