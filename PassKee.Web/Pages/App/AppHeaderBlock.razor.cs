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
