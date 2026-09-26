using Microsoft.AspNetCore.Components;
using PassKee.Web.Core.Services.UI.Modal;

namespace PassKee.Web.Core.Ui.Shared.Components.Modal;

public partial class AppModalFooter : ComponentBase
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
    public bool? IsFullWidthButtons { get; set; }

    private bool ShouldUseFullWidthButtons => IsFullWidthButtons ??
        ((ModalInstance?.Options.Size ?? DeclarativeModal?.Size) == AppModalSize.Small);

    protected string ContainerClasses =>
        ShouldUseFullWidthButtons
            ? "flex items-center gap-2.5 p-4 sm:p-5 pt-0 shrink-0 rounded-b-2xl *:flex-1 *:w-full app-modal-footer-full-width"
            : "flex items-center justify-end gap-2.5 p-4 sm:p-5 pt-0 shrink-0 rounded-b-2xl";
}
