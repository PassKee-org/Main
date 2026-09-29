using System.Collections.Generic;
using Microsoft.AspNetCore.Components;
using PassKee.Web.Models.Vaults;

namespace PassKee.Web.Pages.App;

public partial class CredentialListBlock : ComponentBase
{
    [Parameter]
    public string ActiveDirectoryName { get; set; } = "All Items";

    [Parameter]
    public IEnumerable<DecryptedCredential> Credentials { get; set; } = [];

    [Parameter]
    public EventCallback OnToggleMobileDrawer { get; set; }

    [Parameter]
    public EventCallback OnAddCredential { get; set; }

    [Parameter]
    public EventCallback<DecryptedCredential> OnEditCredential { get; set; }

    [Parameter]
    public EventCallback<DecryptedCredential> OnDeleteCredential { get; set; }
}
