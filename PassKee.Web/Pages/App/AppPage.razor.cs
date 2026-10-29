using System.Linq;
using System.Threading.Tasks;
using Fluxor;
using Microsoft.AspNetCore.Components;
using PassKee.Web.Services.Auth;
using PassKee.Web.Services.UI.Viewport;
using PassKee.Web.Store.Vaults;

namespace PassKee.Web.Pages.App;

public partial class AppPage
{
    [Inject] public IState<VaultsState> VaultsState { get; set; } = null!;
    [Inject] public IInactivityLockService InactivityLockService { get; set; } = null!;
    [Inject] public IViewportService ViewportService { get; set; } = null!;

    protected override void OnInitialized()
    {
        base.OnInitialized();
        if (!AuthState.Value.IsAuthenticated || AuthState.Value.UserPrivateKey == null)
        {
            NavigationManager.NavigateTo("/login");
            return;
        }

        ViewportService.OnViewportChanged += HandleViewportChanged;
        InactivityLockService.Initialize();

        if (!VaultsState.Value.Vaults.Any() && !VaultsState.Value.IsLoading)
        {
            Dispatcher.Dispatch(new LoadVaultsAction());
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);
        if (firstRender)
        {
            await ViewportService.InitializeAsync();
        }
    }

    private void HandleViewportChanged()
    {
        _ = InvokeAsync(StateHasChanged);
    }

    protected override ValueTask DisposeAsyncCore(bool disposing)
    {
        if (disposing)
        {
            ViewportService.OnViewportChanged -= HandleViewportChanged;
        }
        return base.DisposeAsyncCore(disposing);
    }
}
