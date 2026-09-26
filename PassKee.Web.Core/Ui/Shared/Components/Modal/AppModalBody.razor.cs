using Microsoft.AspNetCore.Components;
using PassKee.Web.Core.Services.UI.Modal;

namespace PassKee.Web.Core.Ui.Shared.Components.Modal;

public partial class AppModalBody : ComponentBase
{
    [CascadingParameter]
    public AppModalInstance? ModalInstance { get; set; }

    [CascadingParameter]
    public AppModal? DeclarativeModal { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public string? Class { get; set; }

    [Parameter]
    public bool NoPadding { get; set; } = false;

    [Parameter]
    public bool? IsScrollable { get; set; }

    private bool ResolvedIsScrollable => IsScrollable
        ?? ModalInstance?.Options.IsScrollable
        ?? DeclarativeModal?.IsScrollable
        ?? false;

    private string _bodyClass
    {
        get
        {
            var overflowClass = ResolvedIsScrollable ? "overflow-y-auto" : "overflow-visible";
            var paddingClass = NoPadding ? "" : "p-4 sm:p-5";
            return $"flex-1 {overflowClass} {paddingClass} {Class}".Trim();
        }
    }
}
