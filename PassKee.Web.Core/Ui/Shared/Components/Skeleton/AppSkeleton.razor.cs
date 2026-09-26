using Microsoft.AspNetCore.Components;

namespace PassKee.Web.Core.Ui.Shared.Components;

public partial class AppSkeleton : ComponentBase
{
    [Parameter]
    public bool IsLoaded { get; set; }

    [Parameter]
    public bool DisableAnimation { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public string Class { get; set; } = string.Empty;

    [Parameter]
    public string? Style { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public IDictionary<string, object>? AdditionalAttributes { get; set; }

    protected string ComputedClass
    {
        get
        {
            const string baseClasses = "relative overflow-hidden bg-slate-200/80 dark:bg-slate-700/60 pointer-events-none select-none";

            var shimmerClasses = DisableAnimation
                ? string.Empty
                : "before:absolute before:inset-0 before:-translate-x-full before:animate-[shimmer_1.75s_infinite] before:bg-gradient-to-r before:from-transparent before:via-white/40 dark:before:via-white/10 before:to-transparent";

            return $"{baseClasses} {shimmerClasses} {Class}".Trim();
        }
    }

    protected string LoadedClass => $"transition-opacity duration-300 {Class}".Trim();
}
