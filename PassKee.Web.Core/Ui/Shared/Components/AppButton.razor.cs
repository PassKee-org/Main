using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using PassKee.Web.Core.Ui.Shared.Components.Enums;

namespace PassKee.Web.Core.Ui.Shared.Components;

public partial class AppButton : ComponentBase
{
    [Parameter]
    public ComponentColor Color { get; set; } = ComponentColor.Default;

    [Parameter]
    public ButtonVariant Variant { get; set; } = ButtonVariant.Solid;

    [Parameter]
    public ComponentSize Size { get; set; } = ComponentSize.Medium;

    [Parameter]
    public ButtonRadius Radius { get; set; } = ButtonRadius.Medium;

    [Parameter]
    public ButtonType Type { get; set; } = ButtonType.Button;

    [Parameter]
    public bool Disabled { get; set; }

    [Parameter]
    public bool IsDisabled
    {
        get => Disabled;
        set => Disabled = value;
    }

    [Parameter]
    public bool IsLoading { get; set; }

    [Parameter]
    public bool FullWidth { get; set; }

    [Parameter]
    public bool IsFullWidth
    {
        get => FullWidth;
        set => FullWidth = value;
    }

    [Parameter]
    public bool IconOnly { get; set; }

    [Parameter]
    public bool IsIconOnly
    {
        get => IconOnly;
        set => IconOnly = value;
    }

    protected bool IsIconMode => IconOnly || IsIconOnly;

    [Parameter]
    public string Class { get; set; } = string.Empty;

    [Parameter]
    public EventCallback<MouseEventArgs> OnClick { get; set; }

    [Parameter]
    public RenderFragment? StartContent { get; set; }

    [Parameter]
    public RenderFragment? EndContent { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public IDictionary<string, object>? AdditionalAttributes { get; set; }

    protected string HtmlType => Type switch
    {
        ButtonType.Submit => "submit",
        ButtonType.Reset => "reset",
        _ => "button"
    };

    protected string SpinnerSizeClasses => Size switch
    {
        ComponentSize.Small => "h-3.5 w-3.5",
        ComponentSize.Large => "h-5 w-5",
        _ => "h-4 w-4"
    };

    protected string ComputedClass
    {
        get
        {
            var baseClasses = "inline-flex items-center justify-center font-medium transition-all duration-150 select-none cursor-pointer outline-hidden focus-visible:ring-2 focus-visible:ring-offset-2";
            
            var widthClasses = (FullWidth || IsFullWidth) ? "w-full" : string.Empty;
            
            var disabledClasses = (Disabled || IsLoading) 
                ? "opacity-60 cursor-not-allowed pointer-events-none active:scale-100 shadow-none" 
                : "active:scale-[0.98]";

            var isIconMode = IsIconMode;

            var sizeClasses = Size switch
            {
                ComponentSize.Small => isIconMode 
                    ? "h-8 w-8 min-w-8 max-w-8 shrink-0 aspect-square p-0 text-xs" 
                    : "h-8 px-3 text-xs gap-1.5 shrink-0",
                ComponentSize.Large => isIconMode 
                    ? "h-12 w-12 min-w-12 max-w-12 shrink-0 aspect-square p-0 text-base" 
                    : "h-12 px-6 text-base gap-2.5 shrink-0",
                _ => isIconMode 
                    ? "h-10 w-10 min-w-10 max-w-10 shrink-0 aspect-square p-0 text-sm" 
                    : "h-10 px-4 text-sm gap-2 shrink-0"
            };

            var radiusClasses = Radius switch
            {
                ButtonRadius.None => "rounded-none",
                ButtonRadius.Small => "rounded-md",
                ButtonRadius.Large => "rounded-xl",
                ButtonRadius.Full => "rounded-full",
                _ => "rounded-lg"
            };

            var colorVariantClasses = (Variant, Color) switch
            {
                // Solid
                (ButtonVariant.Solid, ComponentColor.Primary) => 
                    "bg-blue-600 text-white hover:bg-blue-700 dark:bg-blue-600 dark:hover:bg-blue-500 focus-visible:ring-blue-500/50 shadow-2xs",
                (ButtonVariant.Solid, ComponentColor.Secondary) => 
                    "bg-slate-700 text-white hover:bg-slate-800 dark:bg-slate-200 dark:text-slate-800 dark:hover:bg-white focus-visible:ring-slate-500/50 shadow-2xs",
                (ButtonVariant.Solid, ComponentColor.Success) => 
                    "bg-emerald-600 text-white hover:bg-emerald-700 dark:bg-emerald-600 dark:hover:bg-emerald-500 focus-visible:ring-emerald-500/50 shadow-2xs",
                (ButtonVariant.Solid, ComponentColor.Warning) => 
                    "bg-amber-500 text-white hover:bg-amber-600 dark:bg-amber-500 dark:hover:bg-amber-400 focus-visible:ring-amber-500/50 shadow-2xs",
                (ButtonVariant.Solid, ComponentColor.Danger) => 
                    "bg-rose-600 text-white hover:bg-rose-700 dark:bg-rose-600 dark:hover:bg-rose-500 focus-visible:ring-rose-500/50 shadow-2xs",
                (ButtonVariant.Solid, ComponentColor.Info) => 
                    "bg-sky-600 text-white hover:bg-sky-700 dark:bg-sky-600 dark:hover:bg-sky-500 focus-visible:ring-sky-500/50 shadow-2xs",
                (ButtonVariant.Solid, _) => 
                    "bg-slate-200 text-slate-800 hover:bg-slate-300 dark:bg-slate-700 dark:text-slate-100 dark:hover:bg-slate-600 focus-visible:ring-slate-400/50 shadow-2xs",

                // Outlined
                (ButtonVariant.Outlined, ComponentColor.Primary) => 
                    "border border-blue-600 text-blue-600 hover:bg-blue-50 dark:border-blue-500 dark:text-blue-400 dark:hover:bg-blue-950/40 focus-visible:ring-blue-500/50",
                (ButtonVariant.Outlined, ComponentColor.Secondary) => 
                    "border border-slate-700 text-slate-700 hover:bg-slate-100 dark:border-slate-300 dark:text-slate-300 dark:hover:bg-slate-800 focus-visible:ring-slate-500/50",
                (ButtonVariant.Outlined, ComponentColor.Success) => 
                    "border border-emerald-600 text-emerald-600 hover:bg-emerald-50 dark:border-emerald-500 dark:text-emerald-400 dark:hover:bg-emerald-950/40 focus-visible:ring-emerald-500/50",
                (ButtonVariant.Outlined, ComponentColor.Warning) => 
                    "border border-amber-500 text-amber-600 hover:bg-amber-50 dark:border-amber-500 dark:text-amber-400 dark:hover:bg-amber-950/40 focus-visible:ring-amber-500/50",
                (ButtonVariant.Outlined, ComponentColor.Danger) => 
                    "border border-rose-600 text-rose-600 hover:bg-rose-50 dark:border-rose-500 dark:text-rose-400 dark:hover:bg-rose-950/40 focus-visible:ring-rose-500/50",
                (ButtonVariant.Outlined, ComponentColor.Info) => 
                    "border border-sky-600 text-sky-600 hover:bg-sky-50 dark:border-sky-500 dark:text-sky-400 dark:hover:bg-sky-950/40 focus-visible:ring-sky-500/50",
                (ButtonVariant.Outlined, _) => 
                    "border border-slate-300 text-slate-700 hover:bg-slate-100 dark:border-slate-600 dark:text-slate-300 dark:hover:bg-slate-800 focus-visible:ring-slate-400/50",

                // Flat
                (ButtonVariant.Flat, ComponentColor.Primary) => 
                    "bg-blue-50 text-blue-600 hover:bg-blue-100 dark:bg-blue-950/50 dark:text-blue-400 dark:hover:bg-blue-900/50 focus-visible:ring-blue-500/50",
                (ButtonVariant.Flat, ComponentColor.Secondary) => 
                    "bg-slate-200 text-slate-700 hover:bg-slate-300 dark:bg-slate-700 dark:text-slate-300 dark:hover:bg-slate-600 focus-visible:ring-slate-500/50",
                (ButtonVariant.Flat, ComponentColor.Success) => 
                    "bg-emerald-50 text-emerald-600 hover:bg-emerald-100 dark:bg-emerald-950/50 dark:text-emerald-400 dark:hover:bg-emerald-900/50 focus-visible:ring-emerald-500/50",
                (ButtonVariant.Flat, ComponentColor.Warning) => 
                    "bg-amber-50 text-amber-600 hover:bg-amber-100 dark:bg-amber-950/50 dark:text-amber-400 dark:hover:bg-amber-900/50 focus-visible:ring-amber-500/50",
                (ButtonVariant.Flat, ComponentColor.Danger) => 
                    "bg-rose-50 text-rose-600 hover:bg-rose-100 dark:bg-rose-950/50 dark:text-rose-400 dark:hover:bg-rose-900/50 focus-visible:ring-rose-500/50",
                (ButtonVariant.Flat, ComponentColor.Info) => 
                    "bg-sky-50 text-sky-600 hover:bg-sky-100 dark:bg-sky-950/50 dark:text-sky-400 dark:hover:bg-sky-900/50 focus-visible:ring-sky-500/50",
                (ButtonVariant.Flat, _) => 
                    "bg-slate-100 text-slate-700 hover:bg-slate-200 dark:bg-slate-800 dark:text-slate-300 dark:hover:bg-slate-700 focus-visible:ring-slate-400/50",

                // Light
                (ButtonVariant.Light, ComponentColor.Primary) => 
                    "bg-transparent text-blue-600 hover:bg-blue-50 dark:text-blue-400 dark:hover:bg-blue-950/40 focus-visible:ring-blue-500/50",
                (ButtonVariant.Light, ComponentColor.Secondary) => 
                    "bg-transparent text-slate-700 hover:bg-slate-100 dark:text-slate-300 dark:hover:bg-slate-800 focus-visible:ring-slate-500/50",
                (ButtonVariant.Light, ComponentColor.Success) => 
                    "bg-transparent text-emerald-600 hover:bg-emerald-50 dark:text-emerald-400 dark:hover:bg-emerald-950/40 focus-visible:ring-emerald-500/50",
                (ButtonVariant.Light, ComponentColor.Warning) => 
                    "bg-transparent text-amber-600 hover:bg-amber-50 dark:text-amber-400 dark:hover:bg-amber-950/40 focus-visible:ring-amber-500/50",
                (ButtonVariant.Light, ComponentColor.Danger) => 
                    "bg-transparent text-rose-600 hover:bg-rose-50 dark:text-rose-400 dark:hover:bg-rose-950/40 focus-visible:ring-rose-500/50",
                (ButtonVariant.Light, ComponentColor.Info) => 
                    "bg-transparent text-sky-600 hover:bg-sky-50 dark:text-sky-400 dark:hover:bg-sky-950/40 focus-visible:ring-sky-500/50",
                (ButtonVariant.Light, _) => 
                    "bg-transparent text-slate-700 hover:bg-slate-100 dark:text-slate-300 dark:hover:bg-slate-800 focus-visible:ring-slate-400/50",
                _ => 
                    "bg-blue-600 text-white hover:bg-blue-700 dark:bg-blue-600 dark:hover:bg-blue-500 focus-visible:ring-blue-500/50 shadow-2xs"
            };

            return $"{baseClasses} {widthClasses} {disabledClasses} {sizeClasses} {radiusClasses} {colorVariantClasses} {Class}".Trim();
        }
    }

    private async Task HandleClick(MouseEventArgs e)
    {
        if (Disabled || IsLoading)
        {
            return;
        }

        if (OnClick.HasDelegate)
        {
            await OnClick.InvokeAsync(e);
        }
    }
}
