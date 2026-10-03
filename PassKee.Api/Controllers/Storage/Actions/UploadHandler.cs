using System.Threading.Tasks;
using Api.Requests.Abstractions;
using AutoMapper;
using PassKee.Api.Services.Security;
using PassKee.Api.Shared.Models.Storage;
using PassKee.Api.Shared.Models.Storage.Requests;
using PassKee.Business.Common.Constants;
using PassKee.Business.Common.Exceptions.Api;
using PassKee.Business.Services.Http;
using PassKee.Business.Services.Storage;
using PassKee.Orm.Dao.Vaults;

namespace PassKee.Api.Controllers.Storage.Actions;

public class UploadHandler : IAsyncRequestHandler<UploadRequest, StoredFileDto>
{
    private readonly IFileStorageService _fileStorageService;
    private readonly IVaultDao _vaultDao;
    private readonly IApiRequestService _apiRequestService;
    private readonly ISecurityService _securityService;
    private readonly IMapper _mapper;

    public UploadHandler(
        IFileStorageService fileStorageService,
        IVaultDao vaultDao,
        IApiRequestService apiRequestService,
        ISecurityService securityService,
        IMapper mapper)
    {
        _fileStorageService = fileStorageService;
        _vaultDao = vaultDao;
        _apiRequestService = apiRequestService;
        _securityService = securityService;
        _mapper = mapper;
    }

    public async Task<StoredFileDto> ExecuteAsync(UploadRequest request)
    {
        var userId = _apiRequestService.GetCurrentUserId();

        switch (request.EntityType)
        {
            case StorageEntityType.Vault:
                var vault = await _vaultDao.GetById(request.EntityId)
                            ?? throw new RecordNotFoundException("Vault not found");
                await _securityService.CheckAccess(AccessLevel.Write, userId, vault);

                var storedFile = await _fileStorageService.PutFileAsync(vault, request.File);
                return _mapper.Map<StoredFileDto>(storedFile);
            default:
                throw new IncorrectFileException("Unsupported entity type");
        }
    }
}
