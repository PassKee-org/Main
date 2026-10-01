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

public class CreateDirectoryRequestHandler : IAsyncRequestHandler<CreateDirectoryRequest, DirectoryResponse>
{
    private readonly IVaultDao _vaultDao;
    private readonly IDirectoryDao _directoryDao;
    private readonly ISecurityService _securityService;
    private readonly IApiRequestService _apiRequestService;
    private readonly IMapper _mapper;

    public CreateDirectoryRequestHandler(
        IVaultDao vaultDao,
        IDirectoryDao directoryDao,
        ISecurityService securityService,
        IApiRequestService apiRequestService,
        IMapper mapper)
    {
        _vaultDao = vaultDao;
        _directoryDao = directoryDao;
        _securityService = securityService;
        _apiRequestService = apiRequestService;
        _mapper = mapper;
    }

    public async Task<DirectoryResponse> ExecuteAsync(CreateDirectoryRequest request)
    {
        var userId = _apiRequestService.GetCurrentUserId();
        var vault = await _vaultDao.GetById(request.VaultId);
        await _securityService.CheckAccess(AccessLevel.Write, userId, vault);

        if (request.ParentDirectoryId.HasValue)
        {
            var parentDir = await _directoryDao.GetById(request.ParentDirectoryId.Value);
            await _securityService.CheckAccess(AccessLevel.Write, userId, parentDir);
            if (parentDir!.VaultId != vault!.Id)
            {
                throw new HasNoAccessException();
            }
        }

        var dir = await _directoryDao.CreateAsync(request.VaultId, request.ParentDirectoryId, request.EncryptedName);
        return new DirectoryResponse { Directory = _mapper.Map<DirectoryDto>(dir) };
    }
}
