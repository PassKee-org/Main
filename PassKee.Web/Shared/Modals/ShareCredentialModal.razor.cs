using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using PassKee.Api.Shared.Models.Shares;
using PassKee.Api.Shared.Models.Vaults.Payloads;
using PassKee.Business.Common.Constants;
using PassKee.Business.Common.Helpers;
using PassKee.Business.Common.Utils;
using PassKee.Web.Core.Services.UI.Modal;
using PassKee.Web.Core.Services.UI.Toast;
using PassKee.Web.Core.Services.Vaults;
using PassKee.Web.Models.Vaults;
using PassKee.Web.Services.Http;

namespace PassKee.Web.Shared.Modals;

public partial class ShareCredentialModal : ComponentBase
{
    [Parameter] public DecryptedCredential Credential { get; set; } = null!;
    [CascadingParameter] public AppModalInstance ModalInstance { get; set; } = null!;

    [Inject] private ApiService ApiService { get; set; } = null!;
    [Inject] private IVaultCryptoService VaultCryptoService { get; set; } = null!;
    [Inject] private NavigationManager NavigationManager { get; set; } = null!;
    [Inject] private IJSRuntime Js { get; set; } = null!;
    [Inject] private IToastService ToastService { get; set; } = null!;

    protected int SelectedExpirySeconds { get; set; } = 604800; // 7 days by default
    protected string SelectedAudience { get; set; } = "anyone";
    protected bool IsOneTimeView { get; set; } = false;
    protected bool IsCreating { get; set; } = false;
    protected bool IsCopied { get; set; } = false;
    protected string? GeneratedShareUrl { get; set; }

    protected string ItemInitial
    {
        get
        {
            var title = Credential?.Payload?.Title?.Trim();
            if (string.IsNullOrEmpty(title)) return "Ит";
            return title.Length >= 2 ? title[..2] : title;
        }
    }

    protected string? ItemSubtitle => Credential?.Payload switch
    {
        LoginCredentialPayload login when !string.IsNullOrWhiteSpace(login.Username) => login.Username,
        LoginCredentialPayload login when !string.IsNullOrWhiteSpace(login.Website) => login.Website,
        ServerCredentialPayload server when !string.IsNullOrWhiteSpace(server.Username) => server.Username,
        _ => null
    };

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

    protected void ToggleOneTimeView(ChangeEventArgs e)
    {
        IsOneTimeView = e.Value is bool b && b;
        OnSettingChanged();
    }

    protected void OnSettingChanged()
    {
        GeneratedShareUrl = null;
        IsCopied = false;
    }

    protected async Task GenerateAndCopyLinkAsync()
    {
        if (Credential?.Payload == null) return;

        if (!string.IsNullOrWhiteSpace(GeneratedShareUrl))
        {
            await CopyLinkToClipboardAsync();
            return;
        }

        try
        {
            IsCreating = true;
            StateHasChanged();

            // 1. Clone payload and strip files (per requirement / screenshot)
            var json = JsonHelper.SerializeToString(Credential.Payload);
            var cleanPayload = (BaseCredentialPayload)JsonHelper.DeserializeObject(json, Credential.Payload.GetType())!;

            if (cleanPayload is FileCredentialPayload filePayload)
            {
                filePayload.File = null;
            }
            else if (cleanPayload is SshKeyCredentialPayload sshPayload)
            {
                sshPayload.PrivateKeyFile = null;
            }

            cleanPayload.AdditionalFields = cleanPayload.AdditionalFields
                .Where(f => f.Type != CredentialFieldType.File)
                .ToList();

            foreach (var sec in cleanPayload.Sections)
            {
                sec.Fields = sec.Fields.Where(f => f.Type != CredentialFieldType.File).ToList();
            }

            // 2. Generate per-share AES-256 key
            var shareKey = CryptoUtils.GenerateRandomBytes(32);

            // 3. Encrypt payload with per-share key
            var encryptedBody = VaultCryptoService.EncryptCredentialPayload(cleanPayload, shareKey);

            // 4. Send API request to create share
            var request = new CreateCredentialShareRequest
            {
                CredentialId = Credential.Id,
                Type = Credential.Type,
                EncryptedBody = encryptedBody,
                ExpiresInSeconds = SelectedExpirySeconds,
                MaxViews = IsOneTimeView ? 1 : null
            };

            var response = await ApiService.CreateShareAsync(request);
            if (response == null || string.IsNullOrWhiteSpace(response.Token))
            {
                ToastService.ShowError("Не удалось создать ссылку для шеринга");
                return;
            }

            // 5. Construct URL with token and AES key in hash fragment
            var keyFragment = Convert.ToHexString(shareKey).ToLowerInvariant();
            GeneratedShareUrl = $"{NavigationManager.BaseUri}share/{response.Token}#{keyFragment}";

            // 6. Copy to clipboard
            await CopyLinkToClipboardAsync();
        }
        catch (Exception ex)
        {
            ToastService.ShowError($"Ошибка при создании ссылки: {ex.Message}");
        }
        finally
        {
            IsCreating = false;
            StateHasChanged();
        }
    }

    protected async Task CopyLinkToClipboardAsync()
    {
        if (string.IsNullOrWhiteSpace(GeneratedShareUrl)) return;

        try
        {
            await Js.InvokeVoidAsync("navigator.clipboard.writeText", GeneratedShareUrl);
            IsCopied = true;
            ToastService.ShowSuccess("Ссылка скопирована в буфер обмена");
            StateHasChanged();

            await Task.Delay(3000);
            IsCopied = false;
            StateHasChanged();
        }
        catch
        {
            ToastService.ShowWarning("Не удалось автоматически скопировать в буфер обмена. Пожалуйста, скопируйте вручную.");
        }
    }

    protected async Task Close()
    {
        await ModalInstance.CloseAsync();
    }
}

