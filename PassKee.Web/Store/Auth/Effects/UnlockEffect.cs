using System;
using System.Threading.Tasks;
using Fluxor;
using PassKee.Web.Services.Auth;

namespace PassKee.Web.Store.Auth.Effects;

public class UnlockEffect : Effect<UnlockAction>
{
    private readonly IAuthClientService _authService;

    public UnlockEffect(IAuthClientService authService)
    {
        _authService = authService;
    }

    public override async Task HandleAsync(UnlockAction action, IDispatcher dispatcher)
    {
        await Task.Delay(10); // Yield to UI so loading spinner can render

        try
        {
            var loginResult = await _authService.UnlockWithMasterPasswordAsync(action.Password);
            dispatcher.Dispatch(new LoginSuccessAction(loginResult.Response, loginResult.UserPrivateKey, loginResult.UserPublicKey));
        }
        catch (Exception ex)
        {
            dispatcher.Dispatch(new LoginFailureAction(ex.Message));
        }
    }
}
