using System.Threading.Tasks;
using Fluxor;
using Microsoft.AspNetCore.Components;

namespace PassKee.Web.Store.Auth.Effects;

public class LoginSuccessEffect : Effect<LoginSuccessAction>
{
    private readonly NavigationManager _navigationManager;

    public LoginSuccessEffect(NavigationManager navigationManager)
    {
        _navigationManager = navigationManager;
    }

    public override Task HandleAsync(LoginSuccessAction action, IDispatcher dispatcher)
    {
        _navigationManager.NavigateTo("/app");
        return Task.CompletedTask;
    }
}
