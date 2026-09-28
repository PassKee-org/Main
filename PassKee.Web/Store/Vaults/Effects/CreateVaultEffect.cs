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
                _toastService.ShowError("User public key not available.");
                return;
            }

            var created = await _vaultService.CreateVaultAsync(action.Name, userPublicKey);
            if (created != null)
            {
                _toastService.ShowSuccess($"Vault '{created.Name}' created");
                dispatcher.Dispatch(new LoadVaultsAction());
                dispatcher.Dispatch(new SelectVaultAction(created.Id));
                dispatcher.Dispatch(new LoadVaultDetailsAction(created.Id));
            }
        }
        catch
        {
            _toastService.ShowError("Error creating vault");
        }
    }
}
