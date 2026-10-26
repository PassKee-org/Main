using System;
using System.Linq;
using System.Threading.Tasks;
using Fluxor;
using PassKee.Web.Core.Services.UI.Toast;
using PassKee.Web.Services.Vaults;

namespace PassKee.Web.Store.Vaults.Effects;

public class DeleteVaultEffect : Effect<DeleteVaultAction>
{
    private readonly IVaultClientService _vaultService;
    private readonly IToastService _toastService;
    private readonly IState<VaultsState> _vaultsState;

    public DeleteVaultEffect(
        IVaultClientService vaultService,
        IToastService toastService,
        IState<VaultsState> vaultsState)
    {
        _vaultService = vaultService;
        _toastService = toastService;
        _vaultsState = vaultsState;
    }

    public override async Task HandleAsync(DeleteVaultAction action, IDispatcher dispatcher)
    {
        try
        {
            var res = await _vaultService.DeleteVaultAsync(action.VaultId);
            if (res != null)
            {
                _toastService.ShowSuccess("Vault deleted");
                dispatcher.Dispatch(new DeleteVaultSuccessAction(action.VaultId));

                var remainingVaults = _vaultsState.Value.Vaults.Where(v => v.Id != action.VaultId).ToList();
                if (remainingVaults.Count > 0)
                {
                    var nextVault = remainingVaults.First();
                    dispatcher.Dispatch(new SelectVaultAction(nextVault.Id));
                    dispatcher.Dispatch(new LoadVaultDetailsAction(nextVault.Id));
                }
            }
            else
            {
                dispatcher.Dispatch(new DeleteVaultFailureAction());
                _toastService.ShowError("Failed to delete vault");
            }
        }
        catch
        {
            dispatcher.Dispatch(new DeleteVaultFailureAction());
            _toastService.ShowError("Error deleting vault");
        }
    }
}

