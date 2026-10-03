using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using PassKee.Business.Common.Constants;
using PassKee.Web.Models.Vaults;

namespace PassKee.Web.Pages.App;

public partial class CredentialItemBlock : ComponentBase
{
    [Parameter]
    public DecryptedCredential Credential { get; set; } = null!;

    [Parameter]
    public EventCallback<DecryptedCredential> OnView { get; set; }

    [Parameter]
    public EventCallback<DecryptedCredential> OnEdit { get; set; }

    [Parameter]
    public EventCallback<DecryptedCredential> OnDelete { get; set; }

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
        _ => "fa-solid fa-shield-halved"
    };

    protected static string GetCredentialTypeLabel(CredentialType type) => type switch
    {
        CredentialType.Login => "Login",
        CredentialType.Password => "Password",
        CredentialType.Card => "Card",
        CredentialType.SecureNote => "Secure Note",
        CredentialType.File => "File",
        _ => "Item"
    };
}
