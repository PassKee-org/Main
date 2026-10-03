using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using PassKee.Api.Shared.Models.Vaults.Payloads;
using PassKee.Business.Common.Utils.Validators;
using PassKee.Web.Core.Services.UI.Toast;

namespace PassKee.Web.Shared.Modals.Parts;

public partial class CredentialViewFieldBlock : ComponentBase
{
    [Inject] private IJSRuntime Js { get; set; } = null!;
    [Inject] private IToastService ToastService { get; set; } = null!;

    [Parameter] public string? Label { get; set; }
    [Parameter] public string? Value { get; set; }
    [Parameter] public CredentialFieldType? Type { get; set; }
    [Parameter] public bool IsSecret { get; set; }
    [Parameter] public bool IsUrl { get; set; }
    [Parameter] public bool IsMultiline { get; set; }

    private bool _isRevealed;
    private bool _isCopied;

    private bool EffectiveIsSecret => IsSecret || Type is CredentialFieldType.Password or CredentialFieldType.OneTimePassword or CredentialFieldType.SecurityQuestion;
    private bool EffectiveIsUrl => IsUrl || Type == CredentialFieldType.Url;
    private bool EffectiveIsMultiline => IsMultiline || Type == CredentialFieldType.Address;
    private bool EffectiveIsEmail => Type == CredentialFieldType.Email;
    private bool EffectiveIsPhone => Type == CredentialFieldType.Phone;
    private bool EffectiveIsAddress => Type == CredentialFieldType.Address;

    private bool HasValidUrl => EffectiveIsUrl && CredentialFieldValidator.IsValidUrl(Value);
    private bool HasValidEmail => EffectiveIsEmail && CredentialFieldValidator.IsValidEmail(Value);
    private bool HasValidPhone => EffectiveIsPhone && CredentialFieldValidator.IsValidPhone(Value);
    private bool HasValidAddress => EffectiveIsAddress && CredentialFieldValidator.IsValidAddress(Value);

    private string NormalizedUrl => CredentialFieldValidator.NormalizeUrl(Value);
    private string GoogleMapsUrl => CredentialFieldValidator.GetGoogleMapsUrl(Value);

    private void ToggleReveal() => _isRevealed = !_isRevealed;

    private async Task CopyToClipboardAsync()
    {
        if (string.IsNullOrEmpty(Value)) return;

        try
        {
            await Js.InvokeVoidAsync("navigator.clipboard.writeText", Value);
            _isCopied = true;
            ToastService.ShowSuccess($"{(string.IsNullOrWhiteSpace(Label) ? "Value" : Label)} copied to clipboard");
            StateHasChanged();
            await Task.Delay(2000);
            _isCopied = false;
            StateHasChanged();
        }
        catch
        {
            // Ignore if clipboard access is denied
        }
    }
}
