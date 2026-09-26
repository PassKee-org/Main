using Microsoft.AspNetCore.Components;

namespace PassKee.Web.Core.Ui.Shared.Components;

public partial class AppCardHeader : ComponentBase
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
            var defaultPadding = hasPadding ? "" : "p-6 pb-0";

            return $"flex w-full items-center justify-start shrink-0 text-slate-900 dark:text-slate-100 {defaultPadding} {Class}".Trim();
        }
    }
}
