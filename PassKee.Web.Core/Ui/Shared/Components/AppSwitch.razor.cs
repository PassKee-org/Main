using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using PassKee.Web.Core.Ui.Shared.Components.Enums;

namespace PassKee.Web.Core.Ui.Shared.Components;

public partial class AppSwitch : ComponentBase
{
    [Parameter]
    public bool Value { get; set; }

    [Parameter]
    public EventCallback<bool> ValueChanged { get; set; }

    [Parameter]
    public EventCallback<bool> OnChanged { get; set; }

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
    public RenderFragment? ThumbContent { get; set; }

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

    protected string TrackClasses
    {
        get
        {
            var sizeClass = Size switch
            {
                ComponentSize.Small => "w-7 h-4",
                ComponentSize.Large => "w-12 h-7",
                _ => "w-10 h-6"
            };

            var colorClass = Value ? GetActiveColorClass() : "bg-slate-200 dark:bg-slate-700";
            var disabledClass = IsDisabled ? "opacity-50 cursor-not-allowed" : "cursor-pointer";

            return $"relative inline-flex shrink-0 {sizeClass} p-0.5 rounded-full transition-colors duration-200 ease-in-out focus:outline-hidden focus-visible:ring-2 focus-visible:ring-blue-500 focus-visible:ring-offset-2 dark:focus-visible:ring-offset-slate-900 {colorClass} {disabledClass}";
        }
    }

    protected string ThumbClasses
    {
        get
        {
            var (sizeClass, translateClass) = Size switch
            {
                ComponentSize.Small => ("h-3 w-3", Value ? "translate-x-3" : "translate-x-0"),
                ComponentSize.Large => ("h-6 w-6", Value ? "translate-x-5" : "translate-x-0"),
                _ => ("h-5 w-5", Value ? "translate-x-4" : "translate-x-0")
            };

            return $"pointer-events-none inline-flex items-center justify-center transform rounded-full bg-white shadow-sm ring-0 transition duration-200 ease-in-out {sizeClass} {translateClass}";
        }
    }

    protected string LabelClasses => Size switch
    {
        ComponentSize.Small => "text-xs",
        ComponentSize.Large => "text-base",
        _ => "text-sm"
    };

    private string GetActiveColorClass() => Color switch
    {
        ComponentColor.Default => "bg-slate-700 dark:bg-slate-300",
        ComponentColor.Secondary => "bg-purple-600 dark:bg-purple-500",
        ComponentColor.Success => "bg-emerald-600 dark:bg-emerald-500",
        ComponentColor.Warning => "bg-amber-500 dark:bg-amber-400",
        ComponentColor.Danger => "bg-rose-600 dark:bg-rose-500",
        ComponentColor.Info => "bg-cyan-600 dark:bg-cyan-500",
        _ => "bg-blue-600 dark:bg-blue-500"
    };
}
