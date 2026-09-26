using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using PassKee.Web.Core.Services.UI.Modal;

namespace PassKee.Web.Core.Ui.Shared.Components.Modal;

public partial class AppModal : ComponentBase
{
    [Parameter]
    public bool Open { get; set; }

    [Parameter]
    public EventCallback<bool> OpenChanged { get; set; }

    [Parameter]
    public EventCallback OnClose { get; set; }

    [Parameter]
    public AppModalSize Size { get; set; } = AppModalSize.Medium;

    [Parameter]
    public string? ModalClass { get; set; }

    [Parameter]
    public bool HasCloseButton { get; set; } = true;

    [Parameter]
    public bool IsCloseOnBackdropClick { get; set; } = true;

    [Parameter]
    public bool IsCloseOnEscapeKey { get; set; } = true;

    [Parameter]
    public bool IsScrollable { get; set; } = false;

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    private ElementReference _rootElement;
    private bool _prevOpen;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (Open && !_prevOpen)
        {
            _prevOpen = true;
            try
            {
                await _rootElement.FocusAsync();
            }
            catch
            {
                // Ignored if focus fails
            }
        }
        else if (!Open && _prevOpen)
        {
            _prevOpen = false;
        }
    }

    private string _sizeClass => Size switch
    {
        AppModalSize.Small => "max-w-md",
        AppModalSize.Medium => "max-w-lg",
        AppModalSize.Large => "max-w-2xl",
        AppModalSize.ExtraLarge => "max-w-4xl",
        AppModalSize.Full => "max-w-6xl",
        _ => "max-w-lg"
    };

    private string _cardScrollClass => IsScrollable
        ? "max-h-[90vh] flex flex-col overflow-hidden"
        : "overflow-visible";

    public async Task CloseAsync()
    {
        if (Open)
        {
            Open = false;
            if (OpenChanged.HasDelegate)
            {
                await OpenChanged.InvokeAsync(false);
            }
            if (OnClose.HasDelegate)
            {
                await OnClose.InvokeAsync();
            }
            StateHasChanged();
        }
    }

    private async Task OnBackdropClick()
    {
        if (IsCloseOnBackdropClick)
        {
            await CloseAsync();
        }
    }

    private async Task HandleKeyDown(KeyboardEventArgs e)
    {
        if (IsCloseOnEscapeKey && e.Key == "Escape")
        {
            await CloseAsync();
        }
    }
}
