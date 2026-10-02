using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace PassKee.Web.Core.Ui.Shared.Components.Dropdown;

public partial class AppDropdown : ComponentBase
{
    [Parameter]
    public RenderFragment? Trigger { get; set; }

    [Parameter]
    public RenderFragment? Menu { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public DropdownAlignment Alignment { get; set; } = DropdownAlignment.Right;

    [Parameter]
    public string Class { get; set; } = string.Empty;

    [Parameter]
    public string MenuClass { get; set; } = string.Empty;

    [Parameter]
    public bool IsInline { get; set; }

    [Parameter]
    public bool IsDropUp { get; set; }

    [Parameter]
    public string WidthClass { get; set; } = "min-w-[180px]";

    [Parameter]
    public EventCallback<bool> OnOpenChanged { get; set; }

    public bool IsOpen { get; private set; }

    protected string PositionClass => IsInline
        ? "relative mt-1.5"
        : $"absolute {AlignmentClass} {(IsDropUp ? "bottom-full mb-1.5" : "top-full mt-1.5")}";

    protected string AlignmentClass => Alignment switch
    {
        DropdownAlignment.Left => "left-0",
        DropdownAlignment.Right => "right-0",
        _ => "right-0"
    };

    public async Task ToggleOpen()
    {
        IsOpen = !IsOpen;
        if (OnOpenChanged.HasDelegate)
        {
            await OnOpenChanged.InvokeAsync(IsOpen);
        }
        StateHasChanged();
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
            if (OnOpenChanged.HasDelegate)
            {
                await OnOpenChanged.InvokeAsync(false);
            }
            StateHasChanged();
        }
    }

    protected async Task HandleKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Escape" && IsOpen)
        {
            await Close();
        }
    }
}
