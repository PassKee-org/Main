using System;
using System.Threading.Tasks;
using Fluxor;
using PassKee.Web.Core.Services.UI.Toast;
using PassKee.Web.Services.Vaults;

namespace PassKee.Web.Store.Vaults.Effects;

public class CreateCredentialEffect : Effect<CreateCredentialAction>
{
    private readonly IVaultClientService _vaultService;
    private readonly IToastService _toastService;
    private readonly IState<VaultsState> _vaultsState;

    public CreateCredentialEffect(IVaultClientService vaultService, IToastService toastService, IState<VaultsState> vaultsState)
    {
        _vaultService = vaultService;
        _toastService = toastService;
        _vaultsState = vaultsState;
    }

    public override async Task HandleAsync(CreateCredentialAction action, IDispatcher dispatcher)
    {
        try
        {
            var vaultKey = _vaultsState.Value.ActiveVaultKey;
            if (vaultKey == null)
            {
                _toastService.ShowError("Vault is locked or not selected.");
                return;
            }

            var created = await _vaultService.CreateCredentialAsync(action.VaultId, action.DirectoryId, action.Type, action.Payload, vaultKey);
            if (created != null)
            {
                _toastService.ShowSuccess("Credential created");
                dispatcher.Dispatch(new LoadVaultDetailsAction(action.VaultId));
            }
        }
        catch
        {
            _toastService.ShowError("Error creating credential");
        }
    }
}
