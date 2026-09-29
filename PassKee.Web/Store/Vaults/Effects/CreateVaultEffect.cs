using System;
using System.Threading.Tasks;
using Fluxor;
using PassKee.Web.Core.Services.UI.Toast;
using PassKee.Web.Services.Vaults;
using PassKee.Web.Store.Auth;

namespace PassKee.Web.Store.Vaults.Effects;

public class CreateVaultEffect : Effect<CreateVaultAction>
{
    private readonly IVaultClientService _vaultService;
    private readonly IToastService _toastService;
    private readonly IState<AuthState> _authState;

    public CreateVaultEffect(
        IVaultClientService vaultService, 
        IToastService toastService, 
        IState<AuthState> authState)
    {
        _vaultService = vaultService;
        _toastService = toastService;
        _authState = authState;
    }

    public override async Task HandleAsync(CreateVaultAction action, IDispatcher dispatcher)
    {
        try
        {
            var userPublicKey = _authState.Value.UserPublicKey;
            if (userPublicKey == null)
            {
                dispatcher.Dispatch(new CreateVaultFailureAction());
                _toastService.ShowError("User public key not available.");
                return;
            }

            var created = await _vaultService.CreateVaultAsync(action.Name, userPublicKey);
            if (created == null)
            {
                dispatcher.Dispatch(new CreateVaultFailureAction());
                _toastService.ShowError("The server did not return the created vault.");
                return;
            }

            dispatcher.Dispatch(new CreateVaultSuccessAction(created));
            dispatcher.Dispatch(new LoadVaultDetailsAction(created.Id));
            _toastService.ShowSuccess($"Vault '{created.Name}' created");
        }
        catch
        {
            dispatcher.Dispatch(new CreateVaultFailureAction());
            _toastService.ShowError("Couldn't create the vault. Check the connection and try again.");
        }
    }
}
