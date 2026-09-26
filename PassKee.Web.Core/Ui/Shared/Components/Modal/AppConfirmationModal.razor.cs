using Microsoft.AspNetCore.Components;
using PassKee.Web.Core.Services.UI.Modal;
using PassKee.Web.Core.Ui.Shared.Components.Enums;

namespace PassKee.Web.Core.Ui.Shared.Components.Modal;

public partial class AppConfirmationModal : ComponentBase
{
    [Parameter]
    public string? Title { get; set; }

    [Parameter]
    public string Message { get; set; } = string.Empty;

    [Parameter]
    public string? ConfirmText { get; set; }

    [Parameter]
    public string? CancelText { get; set; }

    [Parameter]
    public AppConfirmationType Type { get; set; } = AppConfirmationType.Alert;

    [CascadingParameter]
    public AppModalInstance? ModalInstance { get; set; }

    private string _iconClass => Type switch
    {
        AppConfirmationType.Alert => "fa-solid fa-triangle-exclamation",
        AppConfirmationType.Info => "fa-solid fa-circle-info",
        AppConfirmationType.Danger => "fa-solid fa-circle-exclamation",
        AppConfirmationType.Success => "fa-solid fa-circle-check",
        _ => "fa-solid fa-triangle-exclamation"
    };

    private string _iconBoxClass => Type switch
    {
        AppConfirmationType.Alert => "bg-amber-50 text-amber-600",
        AppConfirmationType.Info => "bg-blue-50 text-blue-600",
        AppConfirmationType.Danger => "bg-rose-50 text-rose-600",
        AppConfirmationType.Success => "bg-emerald-50 text-emerald-600",
        _ => "bg-amber-50 text-amber-600"
    };

    private ComponentColor _confirmButtonColor => Type switch
    {
        AppConfirmationType.Alert => ComponentColor.Warning,
        AppConfirmationType.Info => ComponentColor.Primary,
        AppConfirmationType.Danger => ComponentColor.Danger,
        AppConfirmationType.Success => ComponentColor.Success,
        _ => ComponentColor.Primary
    };

    private void OnCancel()
    {
        ModalInstance?.Close(AppModalResult.Cancel("cancel"));
    }

    private void OnConfirm()
    {
        ModalInstance?.Close(AppModalResult.Ok("confirm"));
    }
}
