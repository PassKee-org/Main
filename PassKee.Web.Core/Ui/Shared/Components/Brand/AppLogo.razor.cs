using Microsoft.AspNetCore.Components;
using PassKee.Web.Core.Ui.Shared.Components.Enums;

namespace PassKee.Web.Core.Ui.Shared.Components.Brand;

public partial class AppLogo : ComponentBase
{
    [Parameter]
    public ComponentSize Size { get; set; } = ComponentSize.Medium;

    [Parameter]
    public bool ShowText { get; set; } = true;

    [Parameter]
    public bool IsDark { get; set; } = false;

    [Parameter]
    public string? Href { get; set; } = "/";

    [Parameter]
    public string? Class { get; set; }

    protected string LogoSrc => IsDark ? "logo-cloud-dark.svg" : "logo-cloud.svg";

    protected string ContainerClasses
    {
        get
        {
            var baseClasses = "inline-flex items-center transition-opacity hover:opacity-95 select-none";
            var gapClass = Size switch
            {
                ComponentSize.Small => "gap-1.5",
                ComponentSize.Large => "gap-3",
                _ => "gap-2.5"
            };

            return string.IsNullOrWhiteSpace(Class)
                ? $"{baseClasses} {gapClass}"
                : $"{baseClasses} {gapClass} {Class}";
        }
    }

    protected string ImageClasses => Size switch
    {
        ComponentSize.Small => "h-5 w-5 shrink-0 object-contain",
        ComponentSize.Large => "h-10 w-10 shrink-0 object-contain",
        _ => "h-8 w-8 shrink-0 object-contain"
    };

    protected string TextClasses
    {
        get
        {
            var fontClasses = Size switch
            {
                ComponentSize.Small => "text-sm font-extrabold tracking-tight",
                ComponentSize.Large => "text-2xl font-extrabold tracking-tight",
                _ => "text-xl font-extrabold tracking-tight"
            };

            var colorClass = IsDark ? "text-white" : "text-slate-900";
            return $"{fontClasses} {colorClass}";
        }
    }

    protected string AccentTextClasses => IsDark ? "text-sky-400" : "text-sky-500";
}

