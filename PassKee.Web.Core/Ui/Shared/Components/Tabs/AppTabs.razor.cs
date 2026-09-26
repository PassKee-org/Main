using Microsoft.AspNetCore.Components;
using PassKee.Web.Core.Ui.Shared.Components.Enums;

namespace PassKee.Web.Core.Ui.Shared.Components.Tabs;

public partial class AppTabs : ComponentBase
{
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public string? ActiveKey { get; set; }

    [Parameter]
    public EventCallback<string?> ActiveKeyChanged { get; set; }

    [Parameter]
    public int? ActiveIndex { get; set; }

    [Parameter]
    public EventCallback<int?> ActiveIndexChanged { get; set; }

    [Parameter]
    public EventCallback<string?> OnTabChanged { get; set; }

    [Parameter]
    public ComponentSize Size { get; set; } = ComponentSize.Medium;

    [Parameter]
    public bool IsFullWidth { get; set; }

    [Parameter]
    public bool FullWidth
    {
        get => IsFullWidth;
        set => IsFullWidth = value;
    }

    [Parameter]
    public bool IsKeepAlive { get; set; }

    [Parameter]
    public bool KeepAlive
    {
        get => IsKeepAlive;
        set => IsKeepAlive = value;
    }

    [Parameter]
    public string? AriaLabel { get; set; }

    [Parameter]
    public string Class { get; set; } = string.Empty;

    [Parameter]
    public string HeaderClass { get; set; } = string.Empty;

    [Parameter]
    public string BodyClass { get; set; } = string.Empty;

    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    private readonly List<AppTab> _tabs = [];
    private string? _prevActiveKey;
    private int? _prevActiveIndex;

    public IReadOnlyList<AppTab> Tabs => _tabs;

    public AppTab? CurrentActiveTab { get; private set; }

    internal void RegisterTab(AppTab tab)
    {
        if (!_tabs.Contains(tab))
        {
            _tabs.Add(tab);
        }
    }

    internal void UnregisterTab(AppTab tab)
    {
        if (_tabs.Remove(tab))
        {
            if (CurrentActiveTab == tab)
            {
                CurrentActiveTab = null;
                DetermineInitialActiveTab();
            }

            StateHasChanged();
        }
    }

    internal void NotifyTabHeaderChanged()
    {
        StateHasChanged();
    }

    private void DetermineInitialActiveTab()
    {
        if (!string.IsNullOrWhiteSpace(ActiveKey))
        {
            var foundByKey = _tabs.FirstOrDefault(t => string.Equals(t.Key, ActiveKey, StringComparison.OrdinalIgnoreCase));
            if (foundByKey != null)
            {
                CurrentActiveTab = foundByKey;
                return;
            }
        }

        if (ActiveIndex.HasValue && ActiveIndex.Value >= 0 && ActiveIndex.Value < _tabs.Count)
        {
            CurrentActiveTab = _tabs[ActiveIndex.Value];
            return;
        }

        if (CurrentActiveTab == null || !_tabs.Contains(CurrentActiveTab))
        {
            CurrentActiveTab = _tabs.FirstOrDefault(t => !t.IsDisabled);
        }
    }

    protected override void OnParametersSet()
    {
        if (ActiveKey != _prevActiveKey || ActiveIndex != _prevActiveIndex)
        {
            _prevActiveKey = ActiveKey;
            _prevActiveIndex = ActiveIndex;
            DetermineInitialActiveTab();
        }
    }

    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender)
        {
            DetermineInitialActiveTab();
            StateHasChanged();
        }
    }

    public bool IsTabActive(AppTab tab)
    {
        if (tab.IsActiveOverride.HasValue)
        {
            return tab.IsActiveOverride.Value;
        }

        if (!string.IsNullOrWhiteSpace(ActiveKey) && !string.IsNullOrWhiteSpace(tab.Key))
        {
            return string.Equals(tab.Key, ActiveKey, StringComparison.OrdinalIgnoreCase);
        }

        return CurrentActiveTab == tab;
    }

    public async Task HandleTabClickAsync(AppTab tab)
    {
        if (tab.IsDisabled || IsTabActive(tab))
        {
            return;
        }

        CurrentActiveTab = tab;

        if (!string.IsNullOrWhiteSpace(tab.Key))
        {
            ActiveKey = tab.Key;
            _prevActiveKey = tab.Key;
            if (ActiveKeyChanged.HasDelegate)
            {
                await ActiveKeyChanged.InvokeAsync(tab.Key);
            }
        }

        var index = _tabs.IndexOf(tab);
        if (index >= 0)
        {
            ActiveIndex = index;
            _prevActiveIndex = index;
            if (ActiveIndexChanged.HasDelegate)
            {
                await ActiveIndexChanged.InvokeAsync(index);
            }
        }

        if (OnTabChanged.HasDelegate)
        {
            await OnTabChanged.InvokeAsync(tab.Key ?? index.ToString());
        }

        StateHasChanged();
    }

    protected string ComputedContainerClass =>
        $"w-full flex flex-col {Class}".Trim();

    protected string ComputedNavClass =>
        $"w-full border-b border-slate-200 dark:border-slate-700/80 {HeaderClass}".Trim();

    protected string ComputedTabListClass
    {
        get
        {
            var baseClass = "flex items-center gap-1 sm:gap-2 overflow-x-auto scrollbar-none pb-px";
            var widthClass = IsFullWidth ? "w-full justify-between" : "w-auto";
            return $"{baseClass} {widthClass}".Trim();
        }
    }

    protected string ComputedBodyClass =>
        $"w-full {BodyClass}".Trim();

    protected string GetTabHeaderItemClass(AppTab tab, bool isActive)
    {
        var sizeClass = Size switch
        {
            ComponentSize.Small => "px-2.5 py-2 text-xs gap-1.5",
            ComponentSize.Large => "px-5 py-3 text-base gap-2.5",
            _ => "px-3.5 py-2.5 text-sm gap-2"
        };

        var stateClass = isActive
            ? "border-blue-600 text-blue-600 dark:border-blue-500 dark:text-blue-400 font-semibold"
            : "border-transparent text-slate-500 hover:text-slate-900 hover:border-slate-300 dark:text-slate-400 dark:hover:text-slate-200 dark:hover:border-slate-600 font-medium";

        var disabledClass = tab.IsDisabled
            ? "opacity-50 cursor-not-allowed pointer-events-none"
            : "cursor-pointer";

        var widthClass = IsFullWidth ? "flex-1 justify-center" : "";

        return $"group inline-flex items-center select-none border-b-2 -mb-px transition-colors whitespace-nowrap focus:outline-none focus-visible:ring-2 focus-visible:ring-blue-500/20 focus-visible:rounded-t {sizeClass} {stateClass} {disabledClass} {widthClass} {tab.Class}".Trim();
    }

    protected static string GetBadgeClass(bool isActive)
    {
        return isActive
            ? "ml-1.5 rounded-full bg-blue-50 dark:bg-blue-950/60 px-2 py-0.5 text-[11px] font-semibold text-blue-700 dark:text-blue-300"
            : "ml-1.5 rounded-full bg-slate-100 dark:bg-slate-800 px-2 py-0.5 text-[11px] font-medium text-slate-600 dark:text-slate-400 group-hover:bg-slate-200/70";
    }
}
