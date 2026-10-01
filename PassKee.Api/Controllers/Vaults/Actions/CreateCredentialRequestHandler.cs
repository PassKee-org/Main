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

public class CreateCredentialRequestHandler : IAsyncRequestHandler<CreateCredentialRequest, CredentialResponse>
{
    private readonly IVaultDao _vaultDao;
    private readonly IDirectoryDao _directoryDao;
    private readonly ICredentialDao _credentialDao;
    private readonly ISecurityService _securityService;
    private readonly IApiRequestService _apiRequestService;
    private readonly IMapper _mapper;

    public CreateCredentialRequestHandler(
        IVaultDao vaultDao,
        IDirectoryDao directoryDao,
        ICredentialDao credentialDao,
        ISecurityService securityService,
        IApiRequestService apiRequestService,
        IMapper mapper)
    {
        _vaultDao = vaultDao;
        _directoryDao = directoryDao;
        _credentialDao = credentialDao;
        _securityService = securityService;
        _apiRequestService = apiRequestService;
        _mapper = mapper;
    }

    public async Task<CredentialResponse> ExecuteAsync(CreateCredentialRequest request)
    {
        var userId = _apiRequestService.GetCurrentUserId();
        var vault = await _vaultDao.GetById(request.VaultId);
        await _securityService.CheckAccess(AccessLevel.Write, userId, vault);

        if (request.DirectoryId.HasValue)
        {
            var dir = await _directoryDao.GetById(request.DirectoryId.Value);
            await _securityService.CheckAccess(AccessLevel.Write, userId, dir);
            if (dir!.VaultId != vault!.Id)
            {
                throw new HasNoAccessException();
            }
        }

        var cred = await _credentialDao.CreateAsync(request.VaultId, request.DirectoryId, request.Type, request.EncryptedBody);
        return new CredentialResponse { Credential = _mapper.Map<CredentialDto>(cred) };
    }
}
