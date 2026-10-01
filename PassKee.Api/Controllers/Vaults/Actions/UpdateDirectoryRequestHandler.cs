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

public class UpdateDirectoryRequestHandler : IAsyncRequestHandler<UpdateDirectoryRequest, DirectoryResponse>
{
    private readonly IDirectoryDao _directoryDao;
    private readonly ISecurityService _securityService;
    private readonly IApiRequestService _apiRequestService;
    private readonly IMapper _mapper;

    public UpdateDirectoryRequestHandler(
        IDirectoryDao directoryDao,
        ISecurityService securityService,
        IApiRequestService apiRequestService,
        IMapper mapper)
    {
        _directoryDao = directoryDao;
        _securityService = securityService;
        _apiRequestService = apiRequestService;
        _mapper = mapper;
    }

    public async Task<DirectoryResponse> ExecuteAsync(UpdateDirectoryRequest request)
    {
        var userId = _apiRequestService.GetCurrentUserId();
        var dir = await _directoryDao.GetById(request.DirectoryId);
        await _securityService.CheckAccess(AccessLevel.Write, userId, dir);

        if (request.ParentDirectoryId.HasValue)
        {
            var parentDir = await _directoryDao.GetById(request.ParentDirectoryId.Value);
            await _securityService.CheckAccess(AccessLevel.Write, userId, parentDir);
            if (parentDir!.VaultId != dir!.VaultId)
            {
                throw new HasNoAccessException();
            }
        }

        var updated = await _directoryDao.UpdateAsync(dir!, request.ParentDirectoryId, request.EncryptedName);
        return new DirectoryResponse { Directory = _mapper.Map<DirectoryDto>(updated) };
    }
}
