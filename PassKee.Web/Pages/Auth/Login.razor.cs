using System;
using Microsoft.AspNetCore.Components;
using PassKee.Web.Components;
using PassKee.Web.Store.Auth;

namespace PassKee.Web.Pages.Auth;

public partial class Login : BaseReactiveComponent
{
    [SupplyParameterFromQuery(Name = "email")]
    private string? QueryEmail { get; set; }

    private string Email { get; set; } = "";
    private string Password { get; set; } = "";
    private string SecretKeyBase64 { get; set; } = "";
    private string _localErrorMessage = "";
    private string ErrorMessage => !string.IsNullOrEmpty(_localErrorMessage) ? _localErrorMessage : (AuthState.Value.ErrorMessage ?? "");
    private bool IsProcessing => AuthState.Value.IsLoading;

    protected override void OnInitialized()
    {
        base.OnInitialized();
        Dispatcher.Dispatch(new ResetAuthStateAction());

        if (!string.IsNullOrWhiteSpace(QueryEmail) && string.IsNullOrWhiteSpace(Email))
        {
            Email = QueryEmail;
        }
    }

    private void LoginAsync()
    {
        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password) || string.IsNullOrWhiteSpace(SecretKeyBase64))
        {
            _localErrorMessage = "All fields are required.";
            return;
        }

        _localErrorMessage = "";
        Dispatcher.Dispatch(new LoginAction(Email, Password, SecretKeyBase64));
    }
}
