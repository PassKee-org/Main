using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using PassKee.Api.Shared.Models.Shares;
using PassKee.Api.Shared.Models.Vaults.Payloads;
using PassKee.Business.Common.Constants;
using PassKee.Web.Core.Services.Vaults;
using PassKee.Web.Services.Http;

namespace PassKee.Web.Pages.Share;

public partial class SharePage : ComponentBase
{
    [Parameter] public string Token { get; set; } = string.Empty;

    [Inject] private ApiService ApiService { get; set; } = null!;
    [Inject] private IVaultCryptoService VaultCryptoService { get; set; } = null!;
    [Inject] private NavigationManager NavigationManager { get; set; } = null!;
    [Inject] private IJSRuntime Js { get; set; } = null!;

    protected bool IsLoading { get; set; } = true;
    protected bool HasError { get; set; } = false;
    protected string ErrorMessage { get; set; } = string.Empty;
    protected BaseCredentialPayload? DecryptedPayload { get; set; }
    protected CredentialType CredentialType { get; set; }
    protected DateTime? ExpiresAt { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await LoadAndDecryptCredentialShareAsync();
    }

    private async Task LoadAndDecryptCredentialShareAsync()
    {
        IsLoading = true;
        HasError = false;
        ErrorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(Token))
        {
            HasError = true;
            ErrorMessage = "Invalid share link.";
            IsLoading = false;
            return;
        }

        var keyFragment = await GetKeyFragmentAsync();
        if (string.IsNullOrWhiteSpace(keyFragment))
        {
            HasError = true;
            ErrorMessage = "Decryption key is missing from the link. Make sure the entire link was copied (including the # character and key).";
            IsLoading = false;
            return;
        }

        byte[] shareKey;
        try
        {
            shareKey = DecodeKey(keyFragment);
        }
        catch
        {
            HasError = true;
            ErrorMessage = "Invalid decryption key format.";
            IsLoading = false;
            return;
        }

        CredentialShareResponse? shareResponse;
        try
        {
            shareResponse = await ApiService.GetShareAsync(Token);
        }
        catch (Exception)
        {
            HasError = true;
            ErrorMessage = "This item is no longer available. The link may have expired or reached its view limit.";
            IsLoading = false;
            return;
        }

        if (shareResponse == null)
        {
            HasError = true;
            ErrorMessage = "This item is no longer available. The link was deleted or has reached its view limit.";
            IsLoading = false;
            return;
        }

        try
        {
            DecryptedPayload = VaultCryptoService.DecryptCredentialPayload(
                shareResponse.EncryptedBody,
                shareResponse.Type,
                shareKey);

            CredentialType = shareResponse.Type;
            ExpiresAt = shareResponse.ExpiresAt;
        }
        catch (Exception)
        {
            HasError = true;
            ErrorMessage = "Failed to decrypt data. The key in the link may be corrupted or incorrect.";
            IsLoading = false;
            return;
        }

        IsLoading = false;
    }

    private async Task<string> GetKeyFragmentAsync()
    {
        var uri = new Uri(NavigationManager.Uri);
        var fragment = uri.Fragment.TrimStart('#');
        if (!string.IsNullOrWhiteSpace(fragment))
        {
            return fragment;
        }

        try
        {
            var hash = await Js.InvokeAsync<string>("eval", "window.location.hash");
            return (hash ?? string.Empty).TrimStart('#');
        }
        catch
        {
            return string.Empty;
        }
    }

    private static byte[] DecodeKey(string keyString)
    {
        var trimmed = keyString.Trim();
        if (trimmed.Length == 64 && trimmed.All(c => Uri.IsHexDigit(c)))
        {
            return Convert.FromHexString(trimmed);
        }

        var base64 = trimmed.Replace('-', '+').Replace('_', '/');
        switch (base64.Length % 4)
        {
            case 2: base64 += "=="; break;
            case 3: base64 += "="; break;
        }
        return Convert.FromBase64String(base64);
    }

    protected static string GetCredentialTypeIcon(CredentialType type) => type switch
    {
        CredentialType.Login => "fa-solid fa-globe",
        CredentialType.Password => "fa-solid fa-key",
        CredentialType.Card => "fa-solid fa-credit-card",
        CredentialType.SecureNote => "fa-solid fa-note-sticky",
        CredentialType.File => "fa-solid fa-file",
        CredentialType.Database => "fa-solid fa-database",
        CredentialType.SshKey => "fa-solid fa-terminal",
        CredentialType.Server => "fa-solid fa-server",
        _ => "fa-solid fa-shield-halved"
    };

    protected static string GetCredentialTypeLabel(CredentialType type) => type switch
    {
        CredentialType.Login => "Login",
        CredentialType.Password => "Password",
        CredentialType.Card => "Card",
        CredentialType.SecureNote => "Secure Note",
        CredentialType.File => "File",
        CredentialType.Database => "Database",
        CredentialType.SshKey => "SSH Key",
        CredentialType.Server => "Server",
        _ => "Item"
    };
}
