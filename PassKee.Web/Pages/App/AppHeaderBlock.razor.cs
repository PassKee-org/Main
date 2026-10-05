using System.Security.Cryptography;
using System.Threading.Tasks;
using Fluxor;
using Microsoft.AspNetCore.Components;
using PassKee.Web.Components;
using PassKee.Web.Services.Storage;
using PassKee.Web.Store.Auth;
using PassKee.Web.Store.Vaults;

namespace PassKee.Web.Pages.App;

public partial class AppHeaderBlock : BaseReactiveComponent
{
    [Inject] public IState<VaultsState> VaultsState { get; set; } = null!;
    [Inject] public ISessionLockStorageService SessionLockStorage { get; set; } = null!;

    [Parameter]
    public EventCallback OnToggleMobileDrawer { get; set; }

    private string _localSearchQuery = string.Empty;
    private System.Threading.CancellationTokenSource? _debounceCts;

    protected override void OnInitialized()
    {
        base.OnInitialized();
        _localSearchQuery = VaultsState.Value.SearchQuery;
    }

    protected override void OnParametersSet()
    {
        base.OnParametersSet();
        if (string.IsNullOrEmpty(VaultsState.Value.SearchQuery) && !string.IsNullOrEmpty(_localSearchQuery))
        {
            _localSearchQuery = string.Empty;
        }
    }

    private void HandleSearchInput(ChangeEventArgs e)
    {
        _localSearchQuery = e.Value?.ToString() ?? string.Empty;
        _debounceCts?.Cancel();
        _debounceCts?.Dispose();
        _debounceCts = new System.Threading.CancellationTokenSource();
        var token = _debounceCts.Token;

        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(200, token);
                await InvokeAsync(() => Dispatcher.Dispatch(new SetSearchQueryAction(_localSearchQuery)));
            }
            catch (System.OperationCanceledException) { }
        });
    }

    private void HandleSearchKeyDown(Microsoft.AspNetCore.Components.Web.KeyboardEventArgs e)
    {
        if (e.Key == "Escape")
        {
            ClearSearch();
        }
        else if (e.Key == "Enter")
        {
            _debounceCts?.Cancel();
            Dispatcher.Dispatch(new SetSearchQueryAction(_localSearchQuery));
        }
    }

    private void ClearSearch()
    {
        _debounceCts?.Cancel();
        _localSearchQuery = string.Empty;
        Dispatcher.Dispatch(new ClearSearchQueryAction());
    }

    protected override ValueTask DisposeAsyncCore(bool disposing)
    {
        if (disposing)
        {
            _debounceCts?.Cancel();
            _debounceCts?.Dispose();
        }
        return base.DisposeAsyncCore(disposing);
    }

    private async Task LogoutAsync()
    {
        await SessionLockStorage.ClearAllSessionDataAsync();
        await LockVaultAsync();
    }

    private async Task LockVaultAsync()
    {
        if (AuthState.Value.UserPrivateKey is { } userPrivateKey)
        {
            CryptographicOperations.ZeroMemory(userPrivateKey);
        }
        if (VaultsState.Value.ActiveVaultKey is { } activeVaultKey)
        {
            CryptographicOperations.ZeroMemory(activeVaultKey);
        }
        Dispatcher.Dispatch(new ResetAuthStateAction());
        Dispatcher.Dispatch(new ResetVaultsStateAction());
        await SessionLockStorage.LockSessionAsync();
        NavigationManager.NavigateTo("/login");
    }
}
