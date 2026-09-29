using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Fluxor;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using PassKee.Web.Components;
using PassKee.Web.Store.Auth;
using PassKee.Web.Store.Vaults;

namespace PassKee.Web.Pages.Auth;

public partial class Register : BaseReactiveComponent
{
    [Inject] private IState<VaultsState> VaultsState { get; set; } = null!;

    private string Email { get; set; } = "";
    private string Password { get; set; } = "";
    private string _localErrorMessage = "";
    private string ErrorMessage => !string.IsNullOrEmpty(_localErrorMessage) ? _localErrorMessage : (AuthState.Value.ErrorMessage ?? "");
    private bool IsProcessing => AuthState.Value.IsLoading;
    private bool ShowSecretKey => AuthState.Value.ShowSecretKey;
    private string SecretKeyBase64 => AuthState.Value.SecretKeyBase64 ?? "";
    private bool HasSavedSecretKey { get; set; } = false;
    private bool IsCopied { get; set; } = false;

    protected override void OnInitialized()
    {
        base.OnInitialized();
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
    }

    private void RegisterAsync()
    {
        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            _localErrorMessage = "Email and Password are required.";
            return;
        }

        _localErrorMessage = "";
        Dispatcher.Dispatch(new RegisterAction(Email, Password));
    }

    private async Task CopySecretKeyAsync()
    {
        try
        {
            await Js.InvokeVoidAsync("navigator.clipboard.writeText", SecretKeyBase64);
            IsCopied = true;
            StateHasChanged();
            await Task.Delay(2500);
            IsCopied = false;
            StateHasChanged();
        }
        catch
        {
            // Ignore if clipboard access is denied
        }
    }

    private void GoToLogin()
    {
        var loginUrl = string.IsNullOrWhiteSpace(Email)
            ? "/login"
            : $"/login?email={Uri.EscapeDataString(Email)}";
        NavigationManager.NavigateTo(loginUrl);
    }
}
