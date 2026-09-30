using System;
using System.Threading.Tasks;
using Fluxor;
using PassKee.Web.Services.Auth;

namespace PassKee.Web.Store.Auth.Effects;

public class RegisterEffect : Effect<RegisterAction>
{
    private readonly IAuthClientService _authService;

    public RegisterEffect(IAuthClientService authService)
    {
        _authService = authService;
    }

    public override async Task HandleAsync(RegisterAction action, IDispatcher dispatcher)
    {
        await Task.Delay(10); // Yield to UI so loading spinner can render

        try
        {
            var result = await _authService.RegisterAsync(action.Email, action.Password, action.SecretKey);
            dispatcher.Dispatch(new RegisterSuccessAction(result.Response, result.SecretKey, result.UserPrivateKey, result.UserPublicKey));
        }
        catch (Exception ex)
        {
            dispatcher.Dispatch(new RegisterFailureAction(ex.Message));
        }
    }
}
