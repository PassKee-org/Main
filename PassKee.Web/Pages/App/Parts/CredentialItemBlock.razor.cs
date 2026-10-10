using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fluxor;
using Microsoft.AspNetCore.Components;
using PassKee.Business.Common.Constants;
using PassKee.Web.Models.Vaults;
using PassKee.Web.Store.Vaults;

namespace PassKee.Web.Pages.App.Parts;

public partial class CredentialItemBlock : ComponentBase
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
    public EventCallback<DecryptedCredential> OnArchive { get; set; }

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
        if (OnView.HasDelegate)
        {
            await OnView.InvokeAsync(Credential);
        }
        else
        {
            await OnEdit.InvokeAsync(Credential);
        }
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

    protected IEnumerable<DecryptedTag> CredentialTags =>
        Credential.Payload?.TagIds != null && Credential.Payload.TagIds.Count > 0
            ? VaultsState.Value.Tags.Where(t => Credential.Payload.TagIds.Contains(t.Id))
            : Enumerable.Empty<DecryptedTag>();
}
