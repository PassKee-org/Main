using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using PassKee.Web.Models.Vaults;
using PassKee.Web.Pages.App.Shared;

namespace PassKee.Web.Pages.App.Mobile.Parts;

public partial class MobileCredentialRowBlock : ComponentBase
{
    [Parameter] public required DecryptedCredential Credential { get; set; }
    [Parameter] public string? DirectoryPath { get; set; }
    [Parameter] public bool ShowDirectoryPath { get; set; }
    [Parameter] public bool IsSelectionMode { get; set; }
    [Parameter] public bool IsSelected { get; set; }
    [Parameter] public bool IsDropUp { get; set; }

    [Parameter] public EventCallback<bool> OnSelectionChanged { get; set; }
    [Parameter] public EventCallback<DecryptedCredential> OnEnterSelectionMode { get; set; }
    [Parameter] public EventCallback<Guid?> OnSelectDirectory { get; set; }
    [Parameter] public EventCallback<DecryptedCredential> OnView { get; set; }
    [Parameter] public EventCallback<DecryptedCredential> OnEdit { get; set; }
    [Parameter] public EventCallback<DecryptedCredential> OnMove { get; set; }
    [Parameter] public EventCallback<DecryptedCredential> OnDuplicate { get; set; }
    [Parameter] public EventCallback<DecryptedCredential> OnShare { get; set; }
    [Parameter] public EventCallback<DecryptedCredential> OnArchive { get; set; }
    [Parameter] public EventCallback<DecryptedCredential> OnDelete { get; set; }
    [Parameter] public EventCallback<(string Value, string Label)> OnCopyValue { get; set; }

    private string? Subtitle => VaultOperationsComponentBase.GetCredentialSubtitle(Credential);

    private (string? Value, string Label) PrimaryCopy =>
        VaultOperationsComponentBase.GetPrimaryCopyableField(Credential);

    private (string? Value, string Label) SecondaryCopy =>
        VaultOperationsComponentBase.GetSecondaryCopyableField(Credential);

    private string? PrimaryCopyValue => PrimaryCopy.Value;
    private string PrimaryCopyLabel => PrimaryCopy.Label;

    private string? SecondaryCopyValue => SecondaryCopy.Value;
    private string SecondaryCopyLabel => SecondaryCopy.Label;

    private async Task HandleRowTap()
    {
        if (IsSelectionMode)
        {
            await OnSelectionChanged.InvokeAsync(!IsSelected);
            return;
        }

        await OnView.InvokeAsync(Credential);
    }

    private async Task HandleDirectoryTap()
    {
        if (OnSelectDirectory.HasDelegate)
        {
            await OnSelectDirectory.InvokeAsync(Credential.DirectoryId);
        }
    }

    private async Task HandleQuickCopyPrimaryAsync()
    {
        if (!string.IsNullOrEmpty(PrimaryCopyValue))
        {
            await OnCopyValue.InvokeAsync((PrimaryCopyValue, PrimaryCopyLabel));
        }
    }

    private async Task HandleQuickCopySecondaryAsync()
    {
        if (!string.IsNullOrEmpty(SecondaryCopyValue))
        {
            await OnCopyValue.InvokeAsync((SecondaryCopyValue, SecondaryCopyLabel));
        }
    }
}

