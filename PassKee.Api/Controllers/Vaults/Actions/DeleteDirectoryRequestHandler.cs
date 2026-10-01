using System.Threading.Tasks;
using Api.Requests.Abstractions;
using PassKee.Api.Services.Security;
using PassKee.Api.Shared.Models.Vaults;
using PassKee.Business.Common.Constants;
using PassKee.Business.Services.Http;
using PassKee.Orm.Dao.Vaults;

namespace PassKee.Api.Controllers.Vaults.Actions;

public class DeleteDirectoryRequestHandler : IAsyncRequestHandler<DeleteDirectoryRequest, ActionResponse>
{
    private readonly IDirectoryDao _directoryDao;
    private readonly ISecurityService _securityService;
    private readonly IApiRequestService _apiRequestService;

    public DeleteDirectoryRequestHandler(
        IDirectoryDao directoryDao,
        ISecurityService securityService,
        IApiRequestService apiRequestService)
    {
        _directoryDao = directoryDao;
        _securityService = securityService;
        _apiRequestService = apiRequestService;
    }

    public async Task<ActionResponse> ExecuteAsync(DeleteDirectoryRequest request)
    {
        var userId = _apiRequestService.GetCurrentUserId();
        var dir = await _directoryDao.GetById(request.DirectoryId);
        await _securityService.CheckAccess(AccessLevel.Write, userId, dir);

        await _directoryDao.DeleteWithDescendantsAsync(dir!);
        return new ActionResponse { Success = true };
    }
}
