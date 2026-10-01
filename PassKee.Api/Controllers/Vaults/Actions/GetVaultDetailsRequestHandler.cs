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

public class GetVaultDetailsRequestHandler : IAsyncRequestHandler<GetVaultDetailsRequest, VaultDetailsResponse>
{
    private readonly IVaultDao _vaultDao;
    private readonly IDirectoryDao _directoryDao;
    private readonly ICredentialDao _credentialDao;
    private readonly ISecurityService _securityService;
    private readonly IApiRequestService _apiRequestService;
    private readonly IMapper _mapper;

    public GetVaultDetailsRequestHandler(
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

    public async Task<VaultDetailsResponse> ExecuteAsync(GetVaultDetailsRequest request)
    {
        var userId = _apiRequestService.GetCurrentUserId();
        
        var vault = await _vaultDao.GetById(request.VaultId);
        await _securityService.CheckAccess(AccessLevel.Read, userId, vault);

        var directories = await _directoryDao.GetByVaultId(request.VaultId);
        var credentials = await _credentialDao.GetByVaultId(request.VaultId);

        return new VaultDetailsResponse
        {
            Vault = _mapper.Map<VaultDto>(vault),
            Directories = _mapper.Map<List<DirectoryDto>>(directories),
            Credentials = _mapper.Map<List<CredentialDto>>(credentials)
        };
    }
}
