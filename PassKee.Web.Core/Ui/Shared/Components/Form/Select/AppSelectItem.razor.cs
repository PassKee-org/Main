using Microsoft.AspNetCore.Components;

namespace PassKee.Web.Core.Ui.Shared.Components.Form.Select;

public partial class AppSelectItem<TValue> : ComponentBase
{
    [CascadingParameter]
    public AppSelect<TValue>? ParentSelect { get; set; }

    [Parameter]
    public TValue? Value { get; set; }

    [Parameter]
    public string? Text { get; set; }

    [Parameter]
    public string? Description { get; set; }

    [Parameter]
    public string? Icon { get; set; }

    [Parameter]
    public bool IsDisabled { get; set; }

    [Parameter]
    public bool Disabled
    {
        get => IsDisabled;
        set => IsDisabled = value;
    }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    public bool IsItemSelected => ParentSelect?.IsSelected(Value) ?? false;

    protected async Task HandleClick()
    {
        if (IsDisabled || ParentSelect == null)
        {
            return;
        }

        await ParentSelect.SelectItemAsync(Value);
    }
}
