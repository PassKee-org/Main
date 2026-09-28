using System;
using System.Threading.Tasks;
using Fluxor;
using PassKee.Web.Core.Services.UI.Toast;
using PassKee.Web.Services.Vaults;

namespace PassKee.Web.Store.Vaults.Effects;

public class UpdateDirectoryEffect : Effect<UpdateDirectoryAction>
{
    private readonly IVaultClientService _vaultService;
    private readonly IToastService _toastService;
    private readonly IState<VaultsState> _vaultsState;

    public UpdateDirectoryEffect(IVaultClientService vaultService, IToastService toastService, IState<VaultsState> vaultsState)
    {
        _vaultService = vaultService;
        _toastService = toastService;
        _vaultsState = vaultsState;
    }

    public override async Task HandleAsync(UpdateDirectoryAction action, IDispatcher dispatcher)
    {
        try
        {
            var vaultKey = _vaultsState.Value.ActiveVaultKey;
            if (vaultKey == null)
            {
                _toastService.ShowError("Vault is locked or not selected.");
                return;
            }

            var updated = await _vaultService.UpdateDirectoryAsync(action.DirectoryId, action.ParentId, action.Name, vaultKey);
            if (updated != null)
            {
                _toastService.ShowSuccess("Directory updated");
                dispatcher.Dispatch(new LoadVaultDetailsAction(action.VaultId));
            }
        }
        catch
        {
            _toastService.ShowError("Error updating directory");
        }
    }
}
