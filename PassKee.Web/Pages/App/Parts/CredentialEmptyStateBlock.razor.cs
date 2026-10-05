using Microsoft.AspNetCore.Components;

namespace PassKee.Web.Pages.App.Parts;

public partial class CredentialEmptyStateBlock : ComponentBase
{
    [Parameter]
    public bool IsSearchActive { get; set; }

    [Parameter]
    public string SearchQuery { get; set; } = string.Empty;

    [Parameter]
    public bool HasMatchingDirectories { get; set; }

    [Parameter]
    public EventCallback OnClearSearch { get; set; }
}
