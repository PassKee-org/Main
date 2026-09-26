namespace PassKee.Web.Core.Services.UI.Modal;

public class AppModalOptions
{
    public AppModalSize Size { get; set; } = AppModalSize.Small;

    public bool HasCloseButton { get; set; } = true;

    public bool IsCloseOnBackdropClick { get; set; } = true;

    public bool IsCloseOnEscapeKey { get; set; } = true;

    public string? ModalClass { get; set; }

    public string? Title { get; set; }

    public bool IsScrollable { get; set; } = false;
}

