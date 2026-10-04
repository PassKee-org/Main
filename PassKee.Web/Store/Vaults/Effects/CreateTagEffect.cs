using System;
using System.Threading.Tasks;
using Fluxor;
using PassKee.Web.Core.Services.UI.Toast;
using PassKee.Web.Models.Vaults;
using PassKee.Web.Services.Vaults;

namespace PassKee.Web.Store.Vaults.Effects;

public class CreateTagEffect : Effect<CreateTagAction>
{
    private readonly IVaultClientService _vaultService;
    private readonly IToastService _toastService;
    private readonly IState<VaultsState> _vaultsState;

    public CreateTagEffect(IVaultClientService vaultService, IToastService toastService, IState<VaultsState> vaultsState)
    {
        _vaultService = vaultService;
        _toastService = toastService;
        _vaultsState = vaultsState;
    }

    public override async Task HandleAsync(CreateTagAction action, IDispatcher dispatcher)
    {
        try
        {
            var vaultKey = _vaultsState.Value.ActiveVaultKey;
            if (vaultKey == null)
            {
                _toastService.ShowError("Vault is locked or not selected.");
                action.Completion?.TrySetResult(null);
                return;
            }

            var created = await _vaultService.CreateTagAsync(action.VaultId, action.Name, vaultKey);
            if (created != null)
            {
                dispatcher.Dispatch(new CreateTagSuccessAction(created));
                action.Completion?.TrySetResult(created);
            }
            else
            {
                _toastService.ShowError("Error creating tag");
                action.Completion?.TrySetResult(null);
            }
        }
        catch (Exception ex)
        {
            _toastService.ShowError("Error creating tag");
            action.Completion?.TrySetException(ex);
        }
    }
}
