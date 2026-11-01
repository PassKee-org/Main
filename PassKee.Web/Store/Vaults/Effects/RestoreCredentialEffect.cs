using System.Threading.Tasks;
using Fluxor;
using PassKee.Web.Core.Services.UI.Toast;
using PassKee.Web.Services.Vaults;

namespace PassKee.Web.Store.Vaults.Effects;

public class RestoreCredentialEffect : Effect<RestoreCredentialAction>
{
    private readonly IVaultClientService _vaultService;
    private readonly IToastService _toastService;

    public RestoreCredentialEffect(IVaultClientService vaultService, IToastService toastService)
    {
        _vaultService = vaultService;
        _toastService = toastService;
    }

    public override async Task HandleAsync(RestoreCredentialAction action, IDispatcher dispatcher)
    {
        try
        {
            var res = await _vaultService.RestoreCredentialAsync(action.CredentialId);
            if (res != null)
            {
                _toastService.ShowSuccess("Credential restored to root");
                dispatcher.Dispatch(new RestoreCredentialSuccessAction(action.VaultId, action.CredentialId));
            }
        }
        catch
        {
            _toastService.ShowError("Error restoring credential");
        }
    }
}

