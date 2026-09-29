using Microsoft.AspNetCore.Components;

namespace PassKee.Web.Pages.App;

public partial class AppHeaderBlock : ComponentBase
{
    [Parameter]
    public bool HasVaults { get; set; }

    [Parameter]
    public EventCallback OnToggleMobileDrawer { get; set; }

    [Parameter]
    public EventCallback OnLock { get; set; }
}
