using System.Threading.Tasks;
using Api.Requests.Abstractions;
using PassKee.Api.Shared.Models.Vaults;
using PassKee.Business.Services.Vaults;
using PassKee.Business.Services.Http;

namespace PassKee.Api.Controllers.Vaults.Actions;

public class DeleteCredentialRequestHandler : IAsyncRequestHandler<DeleteCredentialRequest, ActionResponse>
{
    private readonly IVaultService _vaultService;
    private readonly IApiRequestService _apiRequestService;

    public DeleteCredentialRequestHandler(IVaultService vaultService, IApiRequestService apiRequestService)
    {
        _vaultService = vaultService;
        _apiRequestService = apiRequestService;
    }

    public async Task<ActionResponse> ExecuteAsync(DeleteCredentialRequest request)
    {
        var userId = _apiRequestService.GetCurrentUserId();
        await _vaultService.DeleteCredentialAsync(userId, request.CredentialId);
        return new ActionResponse { Success = true };
    }
}
