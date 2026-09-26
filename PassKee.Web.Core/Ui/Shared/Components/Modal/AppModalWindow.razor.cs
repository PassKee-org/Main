using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using PassKee.Web.Core.Services.UI.Modal;

namespace PassKee.Web.Core.Ui.Shared.Components.Modal;

public partial class AppModalWindow : ComponentBase
{
    [Parameter]
    public required AppModalInstance ModalInstance { get; set; }

    private string _sizeClass => ModalInstance.Options.Size switch
    {
        AppModalSize.Small => "max-w-md",
        AppModalSize.Medium => "max-w-lg",
        AppModalSize.Large => "max-w-2xl",
        AppModalSize.ExtraLarge => "max-w-4xl",
        AppModalSize.Full => "max-w-6xl",
        _ => "max-w-lg"
    };

    private string _cardScrollClass => ModalInstance.Options.IsScrollable
        ? "max-h-[90vh] flex flex-col overflow-hidden"
        : "overflow-visible";

    private void OnBackdropClick()
    {
        if (ModalInstance.Options.IsCloseOnBackdropClick)
        {
            ModalInstance.Close(AppModalResult.Cancel("backdrop"));
        }
    }

    private void HandleKeyDown(KeyboardEventArgs e)
    {
        if (ModalInstance.Options.IsCloseOnEscapeKey && e.Key == "Escape")
        {
            ModalInstance.Close(AppModalResult.Cancel("escape"));
        }
    }
}
