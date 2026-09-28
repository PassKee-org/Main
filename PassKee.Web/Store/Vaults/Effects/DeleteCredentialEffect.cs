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
            var ok = await _vaultService.DeleteCredentialAsync(action.CredentialId);
            if (ok)
            {
                _toastService.ShowSuccess("Credential deleted");
                dispatcher.Dispatch(new LoadVaultDetailsAction(action.VaultId));
            }
        }
        catch
        {
            _toastService.ShowError("Error deleting credential");
        }
    }
}
