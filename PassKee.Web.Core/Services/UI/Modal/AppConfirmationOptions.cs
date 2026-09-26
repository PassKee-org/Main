namespace PassKee.Web.Core.Services.UI.Modal;

public class AppConfirmationOptions
{
    public string Message { get; set; } = string.Empty;

    public string? Title { get; set; }

    public string? ConfirmText { get; set; }

    public string? CancelText { get; set; }

    public AppConfirmationType Type { get; set; } = AppConfirmationType.Alert;

    public AppModalOptions? ModalOptions { get; set; }
}
