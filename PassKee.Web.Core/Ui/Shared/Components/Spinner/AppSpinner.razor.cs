using Microsoft.AspNetCore.Components;
using PassKee.Web.Core.Ui.Shared.Components.Enums;

namespace PassKee.Web.Core.Ui.Shared.Components;

public partial class AppSpinner : ComponentBase
{
    [Parameter]
    public ComponentSize Size { get; set; } = ComponentSize.Medium;

    [Parameter]
    public ComponentColor Color { get; set; } = ComponentColor.Primary;

    [Parameter]
    public SpinnerVariant Variant { get; set; } = SpinnerVariant.Ring;

    [Parameter]
    public string? Label { get; set; }

    [Parameter]
    public ComponentColor LabelColor { get; set; } = ComponentColor.Default;

    [Parameter]
    public string Class { get; set; } = string.Empty;

    [Parameter]
    public string? Style { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public IDictionary<string, object>? AdditionalAttributes { get; set; }

    protected string AriaLabel => string.IsNullOrWhiteSpace(Label) ? "Loading..." : Label;

    protected string ComputedContainerClass
    {
        get
        {
            const string baseClasses = "relative inline-flex items-center justify-center select-none";
            var colorClass = GetColorClass(Color);
            var gapClass = string.IsNullOrWhiteSpace(Label) ? string.Empty : "gap-2";

            return $"{baseClasses} {colorClass} {gapClass} {Class}".Trim();
        }
    }

    protected string ComputedIconSizeClass => Size switch
    {
        ComponentSize.Small => "size-4",
        ComponentSize.Large => "size-10",
        _ => "size-7"
    };

    protected string ComputedDotSizeClass => Size switch
    {
        ComponentSize.Small => "size-1.5",
        ComponentSize.Large => "size-3",
        _ => "size-2"
    };

    protected string ComputedLabelClass
    {
        get
        {
            var sizeClass = Size switch
            {
                ComponentSize.Small => "text-xs",
                ComponentSize.Large => "text-base",
                _ => "text-sm"
            };

            var colorClass = LabelColor switch
            {
                ComponentColor.Primary => "text-blue-600 dark:text-blue-400",
                ComponentColor.Secondary => "text-slate-600 dark:text-slate-300",
                ComponentColor.Success => "text-emerald-600 dark:text-emerald-400",
                ComponentColor.Warning => "text-amber-500 dark:text-amber-400",
                ComponentColor.Danger => "text-rose-600 dark:text-rose-400",
                ComponentColor.Info => "text-sky-600 dark:text-sky-400",
                _ => "text-slate-600 dark:text-slate-300"
            };

            return $"{sizeClass} {colorClass} font-medium".Trim();
        }
    }

    private static string GetColorClass(ComponentColor color) => color switch
    {
        ComponentColor.Primary => "text-blue-600 dark:text-blue-400",
        ComponentColor.Secondary => "text-slate-600 dark:text-slate-300",
        ComponentColor.Success => "text-emerald-600 dark:text-emerald-400",
        ComponentColor.Warning => "text-amber-500 dark:text-amber-400",
        ComponentColor.Danger => "text-rose-600 dark:text-rose-400",
        ComponentColor.Info => "text-sky-600 dark:text-sky-400",
        ComponentColor.Default => "text-current",
        _ => "text-blue-600 dark:text-blue-400"
    };
}
