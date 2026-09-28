using System;
using System.Threading.Tasks;
using Fluxor;
using PassKee.Web.Core.Services.UI.Toast;
using PassKee.Web.Services.Vaults;

namespace PassKee.Web.Store.Vaults.Effects;

public class CreateDirectoryEffect : Effect<CreateDirectoryAction>
{
    private readonly IVaultClientService _vaultService;
    private readonly IToastService _toastService;
    private readonly IState<VaultsState> _vaultsState;

    public CreateDirectoryEffect(IVaultClientService vaultService, IToastService toastService, IState<VaultsState> vaultsState)
    {
        _vaultService = vaultService;
        _toastService = toastService;
        _vaultsState = vaultsState;
    }

    public override async Task HandleAsync(CreateDirectoryAction action, IDispatcher dispatcher)
    {
        try
        {
            var vaultKey = _vaultsState.Value.ActiveVaultKey;
            if (vaultKey == null)
            {
                _toastService.ShowError("Vault is locked or not selected.");
                return;
            }

            var created = await _vaultService.CreateDirectoryAsync(action.VaultId, action.ParentId, action.Name, vaultKey);
            if (created != null)
            {
                _toastService.ShowSuccess("Directory created");
                dispatcher.Dispatch(new LoadVaultDetailsAction(action.VaultId));
            }
        }
        catch
        {
            _toastService.ShowError("Error creating directory");
        }
    }
}
