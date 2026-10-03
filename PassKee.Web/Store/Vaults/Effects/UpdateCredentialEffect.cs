using System;
using System.Threading.Tasks;
using Fluxor;
using PassKee.Web.Services.Vaults;

namespace PassKee.Web.Store.Vaults.Effects;

public class UpdateCredentialEffect : Effect<UpdateCredentialAction>
{
    private readonly IVaultClientService _vaultService;
    private readonly IState<VaultsState> _vaultsState;

    public UpdateCredentialEffect(IVaultClientService vaultService, IState<VaultsState> vaultsState)
    {
        _vaultService = vaultService;
        _vaultsState = vaultsState;
    }

    public override async Task HandleAsync(UpdateCredentialAction action, IDispatcher dispatcher)
    {
        try
        {
            var vaultState = _vaultsState.Value;
            var vaultKey = vaultState.ActiveVaultKey;
            var vaultId = vaultState.ActiveVaultId;
            if (vaultKey == null || !vaultId.HasValue)
            {
                action.Completion.TrySetResult(new CredentialSaveResult(action.RequestId, vaultId, null, null, "Vault is locked or not selected."));
                return;
            }

            var updated = await _vaultService.UpdateCredentialAsync(action.CredentialId, action.DirectoryId, action.Type, action.Payload, vaultKey);
            if (updated == null)
            {
                action.Completion.TrySetResult(new CredentialSaveResult(action.RequestId, vaultId, null, null, "Error updating credential"));
                return;
            }

            dispatcher.Dispatch(new LoadVaultDetailsAction(vaultId.Value));
            action.Completion.TrySetResult(new CredentialSaveResult(action.RequestId, vaultId, updated.Id, updated.DirectoryId, null));
        }
        catch
        {
            action.Completion.TrySetResult(new CredentialSaveResult(action.RequestId, null, null, null, "Error updating credential"));
        }
    }
}
