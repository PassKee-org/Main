using System;
using System.Threading.Tasks;
using Fluxor;
using PassKee.Web.Services.Vaults;

namespace PassKee.Web.Store.Vaults.Effects;

public class LoadArchivedCredentialsEffect : Effect<LoadArchivedCredentialsAction>
{
    private readonly IVaultClientService _vaultService;
    private readonly IState<VaultsState> _vaultsState;

    public LoadArchivedCredentialsEffect(IVaultClientService vaultService, IState<VaultsState> vaultsState)
    {
        _vaultService = vaultService;
        _vaultsState = vaultsState;
    }

    public override async Task HandleAsync(LoadArchivedCredentialsAction action, IDispatcher dispatcher)
    {
        try
        {
            var vaultKey = _vaultsState.Value.ActiveVaultKey;
            if (vaultKey == null)
            {
                throw new InvalidOperationException("Vault key is not available.");
            }

            var credentials = await _vaultService.GetArchivedCredentialsAsync(action.VaultId, vaultKey);
            dispatcher.Dispatch(new LoadArchivedCredentialsSuccessAction(action.VaultId, credentials));
        }
        catch
        {
            dispatcher.Dispatch(new LoadArchivedCredentialsFailureAction());
        }
    }
}

