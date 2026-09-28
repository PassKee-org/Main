using System.Threading.Tasks;
using Api.Requests.Abstractions;
using PassKee.Api.Shared.Models.Vaults;
using PassKee.Business.Services.Vaults;
using PassKee.Business.Services.Http;

namespace PassKee.Api.Controllers.Vaults.Actions;

public class DeleteDirectoryRequestHandler : IAsyncRequestHandler<DeleteDirectoryRequest, ActionResponse>
{
    private readonly IVaultService _vaultService;
    private readonly IApiRequestService _apiRequestService;

    public DeleteDirectoryRequestHandler(IVaultService vaultService, IApiRequestService apiRequestService)
    {
        _vaultService = vaultService;
        _apiRequestService = apiRequestService;
    }

    public async Task<ActionResponse> ExecuteAsync(DeleteDirectoryRequest request)
    {
        var userId = _apiRequestService.GetCurrentUserId();
        await _vaultService.DeleteDirectoryAsync(userId, request.DirectoryId);
        return new ActionResponse { Success = true };
    }
}
