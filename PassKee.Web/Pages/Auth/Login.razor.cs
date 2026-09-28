using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using PassKee.Web.Components;
using PassKee.Web.Services.Storage;
using PassKee.Web.Store.Auth;

namespace PassKee.Web.Pages.Auth;

public partial class Login : BaseReactiveComponent
{
    [Inject] private ISessionLockStorageService SessionLockStorage { get; set; } = null!;

    [SupplyParameterFromQuery(Name = "email")]
    private string? QueryEmail { get; set; }

    private string Email { get; set; } = "";
    private string Password { get; set; } = "";
    private string SecretKeyBase64 { get; set; } = "";
    private bool IsLockedSession { get; set; }
    private string _localErrorMessage = "";
    private string ErrorMessage => !string.IsNullOrEmpty(_localErrorMessage) ? _localErrorMessage : (AuthState.Value.ErrorMessage ?? "");
    private bool IsProcessing => AuthState.Value.IsLoading;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        Dispatcher.Dispatch(new ResetAuthStateAction());

        var lockInfo = await SessionLockStorage.GetSessionLockInfoAsync();
        if (lockInfo != null && !string.IsNullOrWhiteSpace(lockInfo.EncryptedSecretKeyBase64))
        {
            IsLockedSession = true;
            Email = lockInfo.Email;
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
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password) || string.IsNullOrWhiteSpace(SecretKeyBase64))
            {
                _localErrorMessage = "All fields are required.";
                return;
            }

            Dispatcher.Dispatch(new LoginAction(Email, Password, SecretKeyBase64));
        }
    }

    private async Task SwitchAccountAsync()
    {
        await SessionLockStorage.ClearAllSessionDataAsync();
        IsLockedSession = false;
        Email = "";
        Password = "";
        SecretKeyBase64 = "";
        _localErrorMessage = "";
    }
}
