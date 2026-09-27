using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using PassKee.Api.Shared.Dto.RequestsAndResponses.Auth;
using PassKee.Business.Common.Utils;
using PassKee.Web.Services.Http;

namespace PassKee.Web.Pages.Auth;

public partial class Register
{
    [Inject]
    private ApiService ApiService { get; set; } = null!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;

    [Inject]
    private Microsoft.JSInterop.IJSRuntime JSRuntime { get; set; } = null!;

    private string Email { get; set; } = "";
    private string Password { get; set; } = "";
    private string SecretKeyBase64 { get; set; } = "";
    private bool IsProcessing { get; set; } = false;
    private bool ShowSecretKey { get; set; } = false;
    private bool HasSavedSecretKey { get; set; } = false;
    private bool IsCopied { get; set; } = false;
    private string ErrorMessage { get; set; } = "";

    private async Task RegisterAsync()
    {
        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Email and Password are required.";
            return;
        }

        IsProcessing = true;
        ErrorMessage = "";
        StateHasChanged();

        await Task.Delay(10); // yield to UI

        try
        {
            // Prepare client registration data using shared common helper
            var regData = CryptoUtils.PrepareClientRegistration(Password);
            SecretKeyBase64 = Convert.ToBase64String(regData.SecretKey);

            // Submit via ApiService
            var request = new RegisterRequest
            {
                Email = Email,
                AuthHash = regData.AuthHash,
                AuthSalt = regData.AuthSalt,
                KdfParams = regData.KdfParams,
                UserPublicKey = regData.KeyEnvelope.PublicKey,
                EncryptedUserPrivateKey = regData.KeyEnvelope.EncryptedPrivateKey,
                EncryptedUserVaultKey = regData.KeyEnvelope.EncryptedVaultKey
            };

            var response = await ApiService.RegisterAsync(request);
            if (response != null && !string.IsNullOrEmpty(response.AccessToken))
            {
                ShowSecretKey = true;
            }
            else
            {
                ErrorMessage = "Registration failed. Please try again.";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error during registration: {ex.Message}";
        }
        finally
        {
            IsProcessing = false;
        }
    }

    private async Task CopySecretKeyAsync()
    {
        try
        {
            await JSRuntime.InvokeVoidAsync("navigator.clipboard.writeText", SecretKeyBase64);
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

