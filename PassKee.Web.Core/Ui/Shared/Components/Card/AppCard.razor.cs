using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace PassKee.Web.Core.Ui.Shared.Components;

public partial class AppCard : ComponentBase
{
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public string Class { get; set; } = string.Empty;

    [Parameter]
    public bool IsHoverable { get; set; }

    [Parameter]
    public bool Hoverable
    {
        get => IsHoverable;
        set => IsHoverable = value;
    }

    [Parameter]
    public EventCallback<MouseEventArgs> OnClick { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    protected async Task HandleClick(MouseEventArgs e)
    {
        if (OnClick.HasDelegate)
        {
            await OnClick.InvokeAsync(e);
        }
    }

    protected string ComputedClass
    {
        get
        {
            var baseClasses = "relative flex flex-col rounded-2xl border border-slate-200/80 bg-white text-slate-800 shadow-xs transition-all duration-200";
            var interactiveClass = (IsHoverable || OnClick.HasDelegate)
                ? "hover:shadow-md hover:border-slate-300 cursor-pointer"
                : "";

            return $"{baseClasses} {interactiveClass} {Class}".Trim();
        }
    }
}
