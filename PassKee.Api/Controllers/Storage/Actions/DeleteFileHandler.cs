using System.Threading.Tasks;
using Api.Requests.Abstractions;
using PassKee.Api.Services.Storage;
using PassKee.Api.Shared.Models.Storage.Requests;
using PassKee.Api.Shared.Models.Vaults;
using PassKee.Business.Common.Constants;
using PassKee.Business.Common.Exceptions.Api;
using PassKee.Business.Services.Http;
using PassKee.Business.Services.Storage;
using PassKee.Orm.Dao.Storage;

namespace PassKee.Api.Controllers.Storage.Actions;

public class DeleteFileHandler : IAsyncRequestHandler<DeleteFileRequest, ActionResponse>
{
    private readonly IFileStorageService _fileStorageService;
    private readonly IFileStorageDao _fileStorageDao;
    private readonly IFileStorageAccessService _accessService;
    private readonly IApiRequestService _apiRequestService;

    public DeleteFileHandler(
        IFileStorageService fileStorageService,
        IFileStorageDao fileStorageDao,
        IFileStorageAccessService accessService,
        IApiRequestService apiRequestService)
    {
        _fileStorageService = fileStorageService;
        _fileStorageDao = fileStorageDao;
        _accessService = accessService;
        _apiRequestService = apiRequestService;
    }

    public async Task<ActionResponse> ExecuteAsync(DeleteFileRequest request)
    {
        var userId = _apiRequestService.GetCurrentUserId();

        var file = await _fileStorageDao.GetById(request.FileId)
                   ?? throw new RecordNotFoundException("File not found");
        await _accessService.CheckAccessAsync(AccessLevel.Write, userId, file);

        await _fileStorageService.DeleteFileAsync(file);
        return new ActionResponse { Success = true };
    }
}
