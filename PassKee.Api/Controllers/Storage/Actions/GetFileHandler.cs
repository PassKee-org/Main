using System.Threading.Tasks;
using Api.Requests.Abstractions;
using AspNetCore.ApiControllers.Abstractions;
using Microsoft.AspNetCore.Http;
using PassKee.Api.Models.Storage.Requests;
using PassKee.Api.Services.Storage;
using PassKee.Business.Common.Constants;
using PassKee.Business.Common.Exceptions.Api;
using PassKee.Business.Services.Http;
using PassKee.Business.Services.Storage;
using PassKee.Orm.Dao.Storage;

namespace PassKee.Api.Controllers.Storage.Actions;

public class GetFileHandler : IAsyncRequestHandler<GetFileRequest, FileResponse>
{
    private readonly IFileStorageService _fileStorageService;
    private readonly IFileStorageDao _fileStorageDao;
    private readonly IFileStorageAccessService _accessService;
    private readonly IApiRequestService _apiRequestService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public GetFileHandler(
        IFileStorageService fileStorageService,
        IFileStorageDao fileStorageDao,
        IFileStorageAccessService accessService,
        IApiRequestService apiRequestService,
        IHttpContextAccessor httpContextAccessor)
    {
        _fileStorageService = fileStorageService;
        _fileStorageDao = fileStorageDao;
        _accessService = accessService;
        _apiRequestService = apiRequestService;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<FileResponse> ExecuteAsync(GetFileRequest request)
    {
        var userId = _apiRequestService.GetCurrentUserId();

        var file = await _fileStorageDao.GetById(request.FileId)
                   ?? throw new RecordNotFoundException("File not found");
        await _accessService.CheckAccessAsync(AccessLevel.Read, userId, file);

        var fileStream = await _fileStorageService.GetFileStreamAsync(file);

        // The blob is immutable: a new upload always gets a new id.
        _httpContextAccessor.HttpContext?.Response.Headers.Append("Cache-Control", "private,max-age=31536000,immutable");

        return new FileResponse(fileStream, "application/octet-stream");
    }
}
