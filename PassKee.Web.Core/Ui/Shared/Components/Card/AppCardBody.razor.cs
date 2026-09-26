using Microsoft.AspNetCore.Components;

namespace PassKee.Web.Core.Ui.Shared.Components;

public partial class AppCardBody : ComponentBase
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
            var defaultPadding = hasPadding ? "" : "p-6";

            return $"flex flex-col flex-1 w-full text-slate-700 dark:text-slate-200 {defaultPadding} {Class}".Trim();
        }
    }
}
