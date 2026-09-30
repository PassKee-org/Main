using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Fluxor;
using Microsoft.AspNetCore.Components;
using PassKee.Web.Components;
using PassKee.Web.Services.Storage;
using PassKee.Web.Store.Auth;
using PassKee.Web.Store.Vaults;

namespace PassKee.Web.Pages.Auth;

public partial class Login : BaseReactiveComponent
{
    [Inject] private ISessionLockStorageService SessionLockStorage { get; set; } = null!;
    [Inject] private IState<VaultsState> VaultsState { get; set; } = null!;

    [SupplyParameterFromQuery(Name = "email")]
    private string? QueryEmail { get; set; }

    private string Email { get; set; } = "";
    private string Password { get; set; } = "";
    private string SecretKey { get; set; } = "";
    private bool IsLockedSession { get; set; }
    private string _localErrorMessage = "";
    private string ErrorMessage => !string.IsNullOrEmpty(_localErrorMessage) ? _localErrorMessage : (AuthState.Value.ErrorMessage ?? "");
    private bool IsProcessing => AuthState.Value.IsLoading;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
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

        var lockInfo = await SessionLockStorage.GetSessionLockInfoAsync();
        if (lockInfo != null && !string.IsNullOrWhiteSpace(lockInfo.EncryptedSecretKeyBase64))
        {
            Email = lockInfo.Email;
            if (lockInfo.FormatVersion == SessionLockInfo.CurrentFormatVersion && !string.IsNullOrWhiteSpace(lockInfo.SessionUnlockSaltBase64))
            {
                IsLockedSession = true;
            }
            else
            {
                await SessionLockStorage.ClearAllSessionDataAsync();
                _localErrorMessage = "Your saved session uses an older security format. Sign in with your Secret Key.";
            }
        }
        else if (!string.IsNullOrWhiteSpace(QueryEmail) && string.IsNullOrWhiteSpace(Email))
        {
            Email = QueryEmail;
        }
    }

    private void LoginAsync()
    {
        _localErrorMessage = "";

        if (IsLockedSession)
        {
            if (string.IsNullOrWhiteSpace(Password))
            {
                _localErrorMessage = "Master Password is required to unlock your session.";
                return;
            }

            Dispatcher.Dispatch(new UnlockAction(Password));
        }
        else
        {
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password) || string.IsNullOrWhiteSpace(SecretKey))
            {
                _localErrorMessage = "All fields are required.";
                return;
            }

            Dispatcher.Dispatch(new LoginAction(Email, Password, SecretKey));
        }
    }

    private async Task SwitchAccountAsync()
    {
        await SessionLockStorage.ClearAllSessionDataAsync();
        IsLockedSession = false;
        Email = "";
        Password = "";
        SecretKey = "";
        _localErrorMessage = "";
    }
}
