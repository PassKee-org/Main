using System;
using System.Threading.Tasks;
using Fluxor;
using PassKee.Web.Services.Vaults;
using PassKee.Web.Store.Auth;

namespace PassKee.Web.Store.Vaults.Effects;

public class LoadVaultDetailsEffect : Effect<LoadVaultDetailsAction>
{
    private readonly IVaultClientService _vaultService;
    private readonly IState<AuthState> _authState;

    public LoadVaultDetailsEffect(IVaultClientService vaultService, IState<AuthState> authState)
    {
        _vaultService = vaultService;
        _authState = authState;
    }

    public override async Task HandleAsync(LoadVaultDetailsAction action, IDispatcher dispatcher)
    {
        try
        {
            var userPrivateKey = _authState.Value.UserPrivateKey;
            if (userPrivateKey == null)
            {
                throw new InvalidOperationException("User private key is not loaded in memory.");
            }

            var result = await _vaultService.GetVaultDetailsAsync(action.VaultId, userPrivateKey);
            dispatcher.Dispatch(new LoadVaultDetailsSuccessAction(action.VaultId, result.ActiveVaultKey, result.Directories, result.Credentials, result.Tags));
        }
        catch
        {
            dispatcher.Dispatch(new LoadVaultDetailsFailureAction());
        }
    }
}
