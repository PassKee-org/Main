using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fluxor;
using Microsoft.AspNetCore.Components;
using PassKee.Web.Models.Vaults;
using PassKee.Web.Store.Vaults;

namespace PassKee.Web.Pages.App.Desktop.Parts;

public partial class DesktopCredentialItemBlock : ComponentBase
{
    [Inject]
    private IState<VaultsState> VaultsState { get; set; } = null!;

    [Parameter]
    public DecryptedCredential Credential { get; set; } = null!;

    [Parameter]
    public EventCallback<DecryptedCredential> OnView { get; set; }

    [Parameter]
    public EventCallback<DecryptedCredential> OnEdit { get; set; }

    [Parameter]
    public EventCallback<DecryptedCredential> OnMove { get; set; }

    [Parameter]
    public EventCallback<DecryptedCredential> OnDuplicate { get; set; }

    [Parameter]
    public EventCallback<DecryptedCredential> OnShare { get; set; }

    [Parameter]
    public EventCallback<DecryptedCredential> OnArchive { get; set; }

    [Parameter]
    public EventCallback<DecryptedCredential> OnDelete { get; set; }

    [Parameter]
    public bool IsSelectable { get; set; }

    [Parameter]
    public bool IsSelected { get; set; }

    [Parameter]
    public bool HasActiveSelection { get; set; }

    [Parameter]
    public EventCallback<bool> OnSelectionChanged { get; set; }

    [Parameter]
    public string? DirectoryPath { get; set; }

    [Parameter]
    public bool ShowDirectoryPath { get; set; }

    [Parameter]
    public EventCallback<Guid?> OnSelectDirectory { get; set; }

    private async Task HandleDirectoryClick()
    {
        if (OnSelectDirectory.HasDelegate)
        {
            await OnSelectDirectory.InvokeAsync(Credential.DirectoryId);
        }
    }

    private async Task HandleRowClick()
    {
        if (IsSelectable && HasActiveSelection)
        {
            await OnSelectionChanged.InvokeAsync(!IsSelected);
            return;
        }

        if (OnView.HasDelegate)
        {
            await OnView.InvokeAsync(Credential);
        }
        else
        {
            await OnEdit.InvokeAsync(Credential);
        }
    }

    protected IEnumerable<DecryptedTag> CredentialTags =>
        Credential.Payload?.TagIds != null && Credential.Payload.TagIds.Count > 0
            ? VaultsState.Value.Tags.Where(t => Credential.Payload.TagIds.Contains(t.Id))
            : Enumerable.Empty<DecryptedTag>();
}

