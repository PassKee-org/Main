using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using PassKee.Web.Core.Ui.Shared.Components.Enums;

namespace PassKee.Web.Core.Ui.Shared.Components.Popover;

public partial class AppPopover : ComponentBase
{
    [Parameter]
    public RenderFragment? Trigger { get; set; }

    [Parameter]
    public RenderFragment? Content { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public PopoverPlacement Placement { get; set; } = PopoverPlacement.Bottom;

    [Parameter]
    public PopoverTriggerMode TriggerMode { get; set; } = PopoverTriggerMode.ClickOrHover;

    [Parameter]
    public ComponentColor Color { get; set; } = ComponentColor.Default;

    [Parameter]
    public string Class { get; set; } = string.Empty;

    [Parameter]
    public string PanelClass { get; set; } = string.Empty;

    [Parameter]
    public EventCallback<bool> OnOpenChanged { get; set; }

    public bool IsOpen { get; private set; }

    protected bool _isOpenedByClick;

    protected string PlacementClasses => Placement switch
    {
        PopoverPlacement.Bottom => "top-full mt-2 left-1/2 -translate-x-1/2",
        PopoverPlacement.BottomLeft => "top-full mt-2 left-0",
        PopoverPlacement.BottomRight => "top-full mt-2 right-0",
        PopoverPlacement.Top => "bottom-full mb-2 left-1/2 -translate-x-1/2",
        PopoverPlacement.TopLeft => "bottom-full mb-2 left-0",
        PopoverPlacement.TopRight => "bottom-full mb-2 right-0",
        PopoverPlacement.Left => "right-full mr-2 top-1/2 -translate-y-1/2",
        PopoverPlacement.Right => "left-full ml-2 top-1/2 -translate-y-1/2",
        _ => "top-full mt-2 left-1/2 -translate-x-1/2"
    };

    protected string ColorClasses => Color switch
    {
        ComponentColor.Primary => "border-blue-200 dark:border-blue-800 text-blue-900 dark:text-blue-100",
        ComponentColor.Danger => "border-rose-200 dark:border-rose-800 text-rose-900 dark:text-rose-100",
        ComponentColor.Warning => "border-amber-200 dark:border-amber-800 text-amber-900 dark:text-amber-100",
        ComponentColor.Success => "border-emerald-200 dark:border-emerald-800 text-emerald-900 dark:text-emerald-100",
        _ => "text-slate-800 dark:text-slate-100"
    };

    protected async Task HandleTriggerClick(MouseEventArgs e)
    {
        if (TriggerMode == PopoverTriggerMode.Hover)
        {
            return;
        }

        if (IsOpen && _isOpenedByClick)
        {
            await Close();
        }
        else
        {
            _isOpenedByClick = true;
            await Open();
        }
    }

    protected async Task HandleMouseEnter(MouseEventArgs e)
    {
        if (TriggerMode == PopoverTriggerMode.Click)
        {
            return;
        }

        if (!IsOpen)
        {
            _isOpenedByClick = false;
            await Open();
        }
    }

    protected async Task HandleMouseLeave(MouseEventArgs e)
    {
        if (TriggerMode == PopoverTriggerMode.Click)
        {
            return;
        }

        if (IsOpen && !_isOpenedByClick)
        {
            await Close();
        }
    }

    public async Task Open()
    {
        if (!IsOpen)
        {
            IsOpen = true;
            if (OnOpenChanged.HasDelegate)
            {
                await OnOpenChanged.InvokeAsync(true);
            }
            StateHasChanged();
        }
    }

    public async Task Close()
    {
        if (IsOpen)
        {
            IsOpen = false;
            _isOpenedByClick = false;
            if (OnOpenChanged.HasDelegate)
            {
                await OnOpenChanged.InvokeAsync(false);
            }
            StateHasChanged();
        }
    }
}
