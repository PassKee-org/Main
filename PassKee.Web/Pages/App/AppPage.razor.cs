using Fluxor;
using Microsoft.AspNetCore.Components;
using PassKee.Web.Store.Vaults;

namespace PassKee.Web.Pages.App;

public partial class AppPage
{
    [Inject] public IState<VaultsState> VaultsState { get; set; } = null!;

    protected bool IsMobileDrawerOpen { get; set; }
    protected void ToggleMobileDrawer() => IsMobileDrawerOpen = !IsMobileDrawerOpen;
    protected void CloseMobileDrawer() => IsMobileDrawerOpen = false;

    protected override void OnInitialized()
    {
        base.OnInitialized();
        if (!AuthState.Value.IsAuthenticated || AuthState.Value.UserPrivateKey == null)
        {
            NavigationManager.NavigateTo("/login");
            return;
        }

        if (!VaultsState.Value.Vaults.Any() && !VaultsState.Value.IsLoading)
        {
            Dispatcher.Dispatch(new LoadVaultsAction());
        }
    }
}
