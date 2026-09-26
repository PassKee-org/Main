using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using PassKee.Web.Core.Ui.Shared.Components.Enums;

namespace PassKee.Web.Core.Ui.Shared.Components;

public partial class AppCheckbox : ComponentBase
{
    [Parameter]
    public bool Value { get; set; }

    [Parameter]
    public EventCallback<bool> ValueChanged { get; set; }

    [Parameter]
    public EventCallback<bool> OnChanged { get; set; }

    [Parameter]
    public bool IsIndeterminate { get; set; }

    [Parameter]
    public bool Indeterminate
    {
        get => IsIndeterminate;
        set => IsIndeterminate = value;
    }

    [Parameter]
    public bool Disabled { get; set; }

    [Parameter]
    public bool IsDisabled
    {
        get => Disabled;
        set => Disabled = value;
    }

    [Parameter]
    public ComponentColor Color { get; set; } = ComponentColor.Primary;

    [Parameter]
    public ComponentSize Size { get; set; } = ComponentSize.Medium;

    [Parameter]
    public string? Label { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public string Class { get; set; } = string.Empty;

    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    protected async Task HandleToggle()
    {
        if (IsDisabled)
        {
            return;
        }

        Value = !Value;
        IsIndeterminate = false;
        await ValueChanged.InvokeAsync(Value);
        await OnChanged.InvokeAsync(Value);
    }

    protected async Task HandleKeyDown(KeyboardEventArgs e)
    {
        if (IsDisabled)
        {
            return;
        }

        if (e.Key is " " or "Enter")
        {
            await HandleToggle();
        }
    }

    protected string BoxClasses
    {
        get
        {
            var sizeClass = Size switch
            {
                ComponentSize.Small => "w-4 h-4 rounded",
                ComponentSize.Large => "w-6 h-6 rounded-lg",
                _ => "w-5 h-5 rounded-md"
            };

            var isCheckedOrIndeterminate = Value || IsIndeterminate;
            var stateClass = isCheckedOrIndeterminate
                ? GetActiveColorClass()
                : "border-2 border-slate-300 dark:border-slate-600 bg-white dark:bg-slate-800 text-transparent hover:border-slate-400 dark:hover:border-slate-500";

            var disabledClass = IsDisabled ? "opacity-50 cursor-not-allowed" : "cursor-pointer";

            return $"relative inline-flex items-center justify-center shrink-0 border transition-all duration-150 ease-in-out focus:outline-hidden focus-visible:ring-2 focus-visible:ring-blue-500 focus-visible:ring-offset-2 dark:focus-visible:ring-offset-slate-900 {sizeClass} {stateClass} {disabledClass}";
        }
    }

    protected string IconClasses => Size switch
    {
        ComponentSize.Small => "w-2.5 h-2.5",
        ComponentSize.Large => "w-4.5 h-4.5",
        _ => "w-3.5 h-3.5"
    };

    protected string LabelClasses => Size switch
    {
        ComponentSize.Small => "text-xs",
        ComponentSize.Large => "text-base",
        _ => "text-sm"
    };

    private string GetActiveColorClass() => Color switch
    {
        ComponentColor.Default => "bg-slate-700 border-slate-700 text-white dark:bg-slate-300 dark:border-slate-300 dark:text-slate-900",
        ComponentColor.Secondary => "bg-purple-600 border-purple-600 text-white",
        ComponentColor.Success => "bg-emerald-600 border-emerald-600 text-white",
        ComponentColor.Warning => "bg-amber-500 border-amber-500 text-white",
        ComponentColor.Danger => "bg-rose-600 border-rose-600 text-white",
        ComponentColor.Info => "bg-cyan-600 border-cyan-600 text-white",
        _ => "bg-blue-600 border-blue-600 text-white"
    };
}
