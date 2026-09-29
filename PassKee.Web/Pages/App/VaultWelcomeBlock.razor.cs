using Microsoft.AspNetCore.Components;

namespace PassKee.Web.Pages.App;

public partial class VaultWelcomeBlock : ComponentBase
{
    [Parameter]
    public bool IsCreating { get; set; }

    [Parameter]
    public EventCallback OnAddVault { get; set; }
}