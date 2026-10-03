using System;
using System.Threading.Tasks;
using Fluxor;
using PassKee.Web.Services.Vaults;

namespace PassKee.Web.Store.Vaults.Effects;

public class CreateCredentialEffect : Effect<CreateCredentialAction>
{
    private readonly IVaultClientService _vaultService;
    private readonly IState<VaultsState> _vaultsState;

    public CreateCredentialEffect(IVaultClientService vaultService, IState<VaultsState> vaultsState)
    {
        _vaultService = vaultService;
        _vaultsState = vaultsState;
    }

    public override async Task HandleAsync(CreateCredentialAction action, IDispatcher dispatcher)
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

            var created = await _vaultService.CreateCredentialAsync(vaultId.Value, vaultState.SelectedDirectoryId, action.Type, action.Payload, vaultKey);
            if (created == null)
            {
                action.Completion.TrySetResult(new CredentialSaveResult(action.RequestId, vaultId, null, null, "Error creating credential"));
                return;
            }

            dispatcher.Dispatch(new LoadVaultDetailsAction(vaultId.Value));
            action.Completion.TrySetResult(new CredentialSaveResult(action.RequestId, vaultId, created.Id, created.DirectoryId, null));
        }
        catch
        {
            action.Completion.TrySetResult(new CredentialSaveResult(action.RequestId, null, null, null, "Error creating credential"));
        }
    }
}
