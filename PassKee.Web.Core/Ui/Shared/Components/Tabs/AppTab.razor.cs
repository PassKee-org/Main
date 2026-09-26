using Microsoft.AspNetCore.Components;

namespace PassKee.Web.Core.Ui.Shared.Components.Tabs;

public partial class AppTab : ComponentBase, IDisposable
{
    [CascadingParameter]
    internal AppTabs? ParentTabs { get; set; }

    [Parameter]
    public string? Key { get; set; }

    [Parameter]
    public string? Title { get; set; }

    [Parameter]
    public string? Icon { get; set; }

    [Parameter]
    public string? Badge { get; set; }

    [Parameter]
    public string? Href { get; set; }

    [Parameter]
    public bool IsDisabled { get; set; }

    [Parameter]
    public bool Disabled
    {
        get => IsDisabled;
        set => IsDisabled = value;
    }

    [Parameter]
    public bool? IsActive { get; set; }

    [Parameter]
    public bool IsKeepAlive { get; set; }

    [Parameter]
    public bool KeepAlive
    {
        get => IsKeepAlive;
        set => IsKeepAlive = value;
    }

    [Parameter]
    public string Class { get; set; } = string.Empty;

    [Parameter]
    public RenderFragment? TitleTemplate { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    public string Id { get; } = $"app-tab-{Guid.NewGuid():N}";

    public string HeaderId => $"{Id}-header";

    public string PanelId => $"{Id}-panel";

    public bool IsActiveTab => ParentTabs?.IsTabActive(this) ?? (IsActive ?? false);

    public bool? IsActiveOverride => IsActive;

    private string? _prevTitle;
    private string? _prevIcon;
    private string? _prevBadge;
    private bool? _prevDisabled;

    protected override void OnInitialized()
    {
        ParentTabs?.RegisterTab(this);
    }

    protected override void OnParametersSet()
    {
        if (Title != _prevTitle || Icon != _prevIcon || Badge != _prevBadge || IsDisabled != _prevDisabled)
        {
            var isInitial = _prevDisabled == null;
            _prevTitle = Title;
            _prevIcon = Icon;
            _prevBadge = Badge;
            _prevDisabled = IsDisabled;

            if (!isInitial)
            {
                ParentTabs?.NotifyTabHeaderChanged();
            }
        }
    }

    public void Dispose()
    {
        ParentTabs?.UnregisterTab(this);
    }

    protected string ComputedPanelClass
    {
        get
        {
            var hiddenClass = IsActiveTab ? "" : "hidden";
            return $"w-full {hiddenClass} {Class}".Trim();
        }
    }
}
