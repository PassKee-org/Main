using System;
using System.Threading.Tasks;
using Fluxor;
using PassKee.Web.Core.Services.UI.Toast;
using PassKee.Web.Services.Vaults;

namespace PassKee.Web.Store.Vaults.Effects;

public class DeleteCredentialEffect : Effect<DeleteCredentialAction>
{
    private readonly IVaultClientService _vaultService;
    private readonly IToastService _toastService;

    public DeleteCredentialEffect(IVaultClientService vaultService, IToastService toastService)
    {
        _vaultService = vaultService;
        _toastService = toastService;
    }

    public override async Task HandleAsync(DeleteCredentialAction action, IDispatcher dispatcher)
    {
        try
        {
            var res = await _vaultService.DeleteCredentialAsync(action.CredentialId);
            if (res != null && res.Success)
            {
                _toastService.ShowSuccess("Credential deleted");
                dispatcher.Dispatch(new DeleteCredentialSuccessAction(action.VaultId, action.CredentialId));
            }
        }
        catch
        {
            _toastService.ShowError("Error deleting credential");
        }
    }
}
