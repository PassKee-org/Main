using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Fluxor;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using PassKee.Business.Common.Utils;
using PassKee.Web.Components;
using PassKee.Web.Store.Auth;
using PassKee.Web.Store.Vaults;

namespace PassKee.Web.Pages.Auth;

public partial class Register : BaseReactiveComponent
{
    [Inject] private IState<VaultsState> VaultsState { get; set; } = null!;

    private string Email { get; set; } = "";
    private string Password { get; set; } = "";
    private string SecretKey { get; set; } = "";
    private string _localErrorMessage = "";
    private string _passwordErrorMessage = "";
    private string? PasswordErrorMessage => !string.IsNullOrEmpty(_passwordErrorMessage) ? _passwordErrorMessage : null;
    private string ErrorMessage => !string.IsNullOrEmpty(_localErrorMessage) ? _localErrorMessage : (AuthState.Value.ErrorMessage ?? "");
    private bool IsProcessing => AuthState.Value.IsLoading;
    private bool ShowSecretKey => AuthState.Value.ShowSecretKey;
    private string SecretKeyFromState => AuthState.Value.SecretKey ?? "";
    private string SecretKeyDisplay => !string.IsNullOrEmpty(SecretKeyFromState) ? SecretKeyFromState : SecretKey;
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

        SecretKey = CryptoUtils.GenerateSecretKeyString();
    }

    private void GeneratePassword()
    {
        Password = SecurityUtil.GenerateStrongPassword(16);
        _passwordErrorMessage = "";
        _localErrorMessage = "";
        StateHasChanged();
    }

    private void GenerateNewSecretKey()
    {
        SecretKey = CryptoUtils.GenerateSecretKeyString();
        StateHasChanged();
    }

    private void RegisterAsync()
    {
        _localErrorMessage = "";
        _passwordErrorMessage = "";

        if (string.IsNullOrWhiteSpace(Email))
        {
            _localErrorMessage = "Email Address is required.";
            return;
        }

        var validationResult = PasswordValidator.Validate(Password);
        if (!validationResult.IsValid)
        {
            _passwordErrorMessage = validationResult.ErrorMessage ?? "Password does not meet complexity requirements.";
            _localErrorMessage = _passwordErrorMessage;
            return;
        }

        if (string.IsNullOrWhiteSpace(SecretKey))
        {
            SecretKey = CryptoUtils.GenerateSecretKeyString();
        }

        Dispatcher.Dispatch(new RegisterAction(Email.Trim(), Password, SecretKey.Trim()));
    }

    private async Task CopySecretKeyAsync()
    {
        try
        {
            await Js.InvokeVoidAsync("navigator.clipboard.writeText", SecretKeyDisplay);
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
