using Microsoft.AspNetCore.Components;

namespace PassKee.Web.Core.Ui.Shared.Components;

public partial class AppCardFooter : ComponentBase
{
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public string Class { get; set; } = string.Empty;

    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    protected string ComputedClass
    {
        get
        {
            var hasPadding = !string.IsNullOrWhiteSpace(Class) &&
                (Class.Contains("p-") || Class.Contains("px-") || Class.Contains("py-") || Class.Contains("pb-") || Class.Contains("pt-"));
            var defaultPadding = hasPadding ? "" : "p-6 pt-0";

            return $"flex w-full items-center justify-start shrink-0 text-slate-600 dark:text-slate-300 {defaultPadding} {Class}".Trim();
        }
    }
}
