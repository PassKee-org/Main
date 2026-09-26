using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using PassKee.Web.Core.Ui.Shared.Components.Enums;

namespace PassKee.Web.Core.Ui.Shared.Components;

public partial class AppBadge : ComponentBase
{
    [Parameter]
    public ComponentColor Color { get; set; } = ComponentColor.Default;

    [Parameter]
    public BadgeVariant Variant { get; set; } = BadgeVariant.Flat;

    [Parameter]
    public ComponentSize Size { get; set; } = ComponentSize.Medium;

    [Parameter]
    public BadgeRadius Radius { get; set; } = BadgeRadius.Full;

    [Parameter]
    public string? Text { get; set; }

    [Parameter]
    public string? Icon { get; set; }

    [Parameter]
    public bool IsDot { get; set; }

    [Parameter]
    public bool IsDismissible { get; set; }

    [Parameter]
    public bool IsDisabled { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public RenderFragment? StartContent { get; set; }

    [Parameter]
    public RenderFragment? EndContent { get; set; }

    [Parameter]
    public EventCallback<MouseEventArgs> OnClick { get; set; }

    [Parameter]
    public EventCallback<MouseEventArgs> OnClose { get; set; }

    [Parameter]
    public string Class { get; set; } = string.Empty;

    [Parameter]
    public string? Style { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public IDictionary<string, object>? AdditionalAttributes { get; set; }

    protected bool IsClickable => OnClick.HasDelegate && !IsDisabled;

    protected string ComputedClass
    {
        get
        {
            const string baseClasses = "inline-flex items-center font-medium select-none whitespace-nowrap transition-all duration-150";

            var interactiveClasses = IsClickable
                ? "cursor-pointer outline-hidden focus-visible:ring-2 focus-visible:ring-offset-1 active:scale-[0.98]"
                : string.Empty;

            var disabledClasses = IsDisabled
                ? "opacity-60 cursor-not-allowed pointer-events-none"
                : string.Empty;

            var sizeClasses = Size switch
            {
                ComponentSize.Small => "h-5 px-2 text-[11px] leading-none gap-1",
                ComponentSize.Large => "h-7 px-3 text-sm leading-none gap-2",
                _ => "h-6 px-2.5 text-xs leading-none gap-1.5"
            };

            var radiusClasses = Radius switch
            {
                BadgeRadius.None => "rounded-none",
                BadgeRadius.Small => "rounded-md",
                BadgeRadius.Medium => "rounded-lg",
                BadgeRadius.Large => "rounded-xl",
                _ => "rounded-full"
            };

            var colorVariantClasses = (Variant, Color) switch
            {
                // Flat
                (BadgeVariant.Flat, ComponentColor.Primary) =>
                    "bg-blue-50 text-blue-700 border border-blue-200/80 dark:bg-blue-950/50 dark:text-blue-300 dark:border-blue-800/60",
                (BadgeVariant.Flat, ComponentColor.Secondary) =>
                    "bg-slate-100 text-slate-700 border border-slate-200/80 dark:bg-slate-800 dark:text-slate-300 dark:border-slate-700",
                (BadgeVariant.Flat, ComponentColor.Success) =>
                    "bg-emerald-50 text-emerald-700 border border-emerald-200/80 dark:bg-emerald-950/50 dark:text-emerald-300 dark:border-emerald-800/60",
                (BadgeVariant.Flat, ComponentColor.Warning) =>
                    "bg-amber-50 text-amber-700 border border-amber-200/80 dark:bg-amber-950/50 dark:text-amber-300 dark:border-amber-800/60",
                (BadgeVariant.Flat, ComponentColor.Danger) =>
                    "bg-rose-50 text-rose-700 border border-rose-200/80 dark:bg-rose-950/50 dark:text-rose-300 dark:border-rose-800/60",
                (BadgeVariant.Flat, ComponentColor.Info) =>
                    "bg-sky-50 text-sky-700 border border-sky-200/80 dark:bg-sky-950/50 dark:text-sky-300 dark:border-sky-800/60",
                (BadgeVariant.Flat, _) =>
                    "bg-slate-100 text-slate-600 border border-slate-200/80 dark:bg-slate-800 dark:text-slate-300 dark:border-slate-700",

                // Solid
                (BadgeVariant.Solid, ComponentColor.Primary) =>
                    "bg-blue-600 text-white shadow-2xs dark:bg-blue-600",
                (BadgeVariant.Solid, ComponentColor.Secondary) =>
                    "bg-slate-700 text-white shadow-2xs dark:bg-slate-200 dark:text-slate-800",
                (BadgeVariant.Solid, ComponentColor.Success) =>
                    "bg-emerald-600 text-white shadow-2xs dark:bg-emerald-600",
                (BadgeVariant.Solid, ComponentColor.Warning) =>
                    "bg-amber-500 text-white shadow-2xs dark:bg-amber-500",
                (BadgeVariant.Solid, ComponentColor.Danger) =>
                    "bg-rose-600 text-white shadow-2xs dark:bg-rose-600",
                (BadgeVariant.Solid, ComponentColor.Info) =>
                    "bg-sky-600 text-white shadow-2xs dark:bg-sky-600",
                (BadgeVariant.Solid, _) =>
                    "bg-slate-600 text-white shadow-2xs dark:bg-slate-600",

                // Outlined
                (BadgeVariant.Outlined, ComponentColor.Primary) =>
                    "border border-blue-600 text-blue-600 dark:border-blue-400 dark:text-blue-400",
                (BadgeVariant.Outlined, ComponentColor.Secondary) =>
                    "border border-slate-400 text-slate-700 dark:border-slate-500 dark:text-slate-300",
                (BadgeVariant.Outlined, ComponentColor.Success) =>
                    "border border-emerald-600 text-emerald-600 dark:border-emerald-400 dark:text-emerald-400",
                (BadgeVariant.Outlined, ComponentColor.Warning) =>
                    "border border-amber-500 text-amber-600 dark:border-amber-400 dark:text-amber-400",
                (BadgeVariant.Outlined, ComponentColor.Danger) =>
                    "border border-rose-600 text-rose-600 dark:border-rose-400 dark:text-rose-400",
                (BadgeVariant.Outlined, ComponentColor.Info) =>
                    "border border-sky-600 text-sky-600 dark:border-sky-400 dark:text-sky-400",
                (BadgeVariant.Outlined, _) =>
                    "border border-slate-300 text-slate-600 dark:border-slate-600 dark:text-slate-300",

                // Light
                (BadgeVariant.Light, ComponentColor.Primary) =>
                    "bg-transparent text-blue-600 hover:bg-blue-50/60 dark:text-blue-400 dark:hover:bg-blue-950/40",
                (BadgeVariant.Light, ComponentColor.Secondary) =>
                    "bg-transparent text-slate-600 hover:bg-slate-100/60 dark:text-slate-300 dark:hover:bg-slate-800/40",
                (BadgeVariant.Light, ComponentColor.Success) =>
                    "bg-transparent text-emerald-600 hover:bg-emerald-50/60 dark:text-emerald-400 dark:hover:bg-emerald-950/40",
                (BadgeVariant.Light, ComponentColor.Warning) =>
                    "bg-transparent text-amber-600 hover:bg-amber-50/60 dark:text-amber-400 dark:hover:bg-amber-950/40",
                (BadgeVariant.Light, ComponentColor.Danger) =>
                    "bg-transparent text-rose-600 hover:bg-rose-50/60 dark:text-rose-400 dark:hover:bg-rose-950/40",
                (BadgeVariant.Light, ComponentColor.Info) =>
                    "bg-transparent text-sky-600 hover:bg-sky-50/60 dark:text-sky-400 dark:hover:bg-sky-950/40",
                (BadgeVariant.Light, _) =>
                    "bg-transparent text-slate-600 hover:bg-slate-100/60 dark:text-slate-300 dark:hover:bg-slate-800/40",
                _ =>
                    "bg-slate-100 text-slate-600 border border-slate-200/80 dark:bg-slate-800 dark:text-slate-300 dark:border-slate-700"
            };

            return $"{baseClasses} {interactiveClasses} {disabledClasses} {sizeClasses} {radiusClasses} {colorVariantClasses} {Class}".Trim();
        }
    }

    protected string DotClass
    {
        get
        {
            var dotSize = Size switch
            {
                ComponentSize.Large => "h-2 w-2",
                _ => "h-1.5 w-1.5"
            };

            var dotColor = Color switch
            {
                ComponentColor.Primary => "bg-blue-500",
                ComponentColor.Secondary => "bg-slate-500",
                ComponentColor.Success => "bg-emerald-500",
                ComponentColor.Warning => "bg-amber-500",
                ComponentColor.Danger => "bg-rose-500",
                ComponentColor.Info => "bg-sky-500",
                _ => "bg-slate-400"
            };

            return $"rounded-full shrink-0 {dotSize} {dotColor}".Trim();
        }
    }

    protected string CloseButtonClass =>
        "ml-1 -mr-0.5 inline-flex items-center justify-center rounded-full opacity-60 hover:opacity-100 transition-opacity focus:outline-hidden cursor-pointer";

    protected async Task HandleClickAsync(MouseEventArgs e)
    {
        if (IsDisabled)
        {
            return;
        }

        if (OnClick.HasDelegate)
        {
            await OnClick.InvokeAsync(e);
        }
    }

    protected async Task HandleCloseClickAsync(MouseEventArgs e)
    {
        if (IsDisabled)
        {
            return;
        }

        if (OnClose.HasDelegate)
        {
            await OnClose.InvokeAsync(e);
        }
    }
}

