using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using PassKee.Web.Core.Ui.Shared.Components.Enums;

namespace PassKee.Web.Core.Ui.Shared.Components.Dropdown;

public partial class AppDropdownItem : ComponentBase
{
    [CascadingParameter]
    public AppDropdown? Dropdown { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public RenderFragment? StartContent { get; set; }

    [Parameter]
    public RenderFragment? EndContent { get; set; }

    [Parameter]
    public string? Text { get; set; }

    [Parameter]
    public string? Description { get; set; }

    [Parameter]
    public string? Icon { get; set; }

    [Parameter]
    public EventCallback<MouseEventArgs> OnClick { get; set; }

    [Parameter]
    public ComponentColor Color { get; set; } = ComponentColor.Default;

    [Parameter]
    public bool IsDisabled { get; set; }

    [Parameter]
    public bool Disabled
    {
        get => IsDisabled;
        set => IsDisabled = value;
    }

    [Parameter]
    public bool ShowDivider { get; set; }

    [Parameter]
    public bool IsActive { get; set; }

    [Parameter]
    public string Class { get; set; } = string.Empty;

    protected string ComputedClass
    {
        get
        {
            var baseClasses = "w-full px-3 py-2 rounded-lg text-xs md:text-sm flex items-center justify-between gap-2 text-left transition-colors cursor-pointer select-none";
            var colorClasses = Color switch
            {
                ComponentColor.Danger => "text-rose-600 dark:text-rose-400 hover:bg-rose-50 dark:hover:bg-rose-950/40",
                ComponentColor.Warning => "text-amber-600 dark:text-amber-400 hover:bg-amber-50 dark:hover:bg-amber-950/40",
                ComponentColor.Primary => "text-blue-600 dark:text-blue-400 hover:bg-blue-50 dark:hover:bg-blue-950/40",
                ComponentColor.Success => "text-emerald-600 dark:text-emerald-400 hover:bg-emerald-50 dark:hover:bg-emerald-950/40",
                ComponentColor.Info => "text-cyan-600 dark:text-cyan-400 hover:bg-cyan-50 dark:hover:bg-cyan-950/40",
                _ => "text-slate-700 dark:text-slate-200 hover:bg-slate-100 dark:hover:bg-slate-700/60"
            };
            var disabledClasses = IsDisabled ? "opacity-50 cursor-not-allowed pointer-events-none" : "";
            var activeClasses = IsActive ? "bg-blue-50 dark:bg-blue-950/40 font-semibold text-blue-600 dark:text-blue-400" : "";

            return $"{baseClasses} {colorClasses} {activeClasses} {disabledClasses} {Class}".Trim();
        }
    }

    protected async Task HandleClick(MouseEventArgs e)
    {
        if (IsDisabled)
        {
            return;
        }

        if (OnClick.HasDelegate)
        {
            await OnClick.InvokeAsync(e);
        }

        if (Dropdown != null)
        {
            await Dropdown.Close();
        }
    }
}
