using Microsoft.AspNetCore.Components;
using PassKee.Business.Extensions;
using PassKee.Web.Core.Ui.Shared.Components.Form.Select.Core;

namespace PassKee.Web.Core.Ui.Shared.Components.Form.Select;

public partial class EnumSelect<TItem> : AppBaseSelect where TItem : struct, IConvertible
{
    [Parameter]
    public TItem? Value
    {
        get => _value;
        set => _value = value;
    }

    [Parameter]
    public EventCallback<TItem?> ValueChanged { get; set; }

    [Parameter]
    public EventCallback<TItem?> OnChanged { get; set; }

    [Parameter]
    public ICollection<TItem>? Values { get; set; }

    [Parameter]
    public EventCallback<ICollection<TItem>> ValuesChanged { get; set; }

    [Parameter]
    public EventCallback<ICollection<TItem>> OnMultipleChanged { get; set; }

    [Parameter]
    public string? Style { get; set; }

    [Parameter]
    public bool AllowClear
    {
        get => IsClearable;
        set => IsClearable = value;
    }

    [Parameter]
    public string? Name { get; set; }

    [Parameter]
    public ICollection<TItem> AllowedValues { get; set; } = new List<TItem>();

    [Parameter]
    public Func<TItem, string>? ItemIcon { get; set; }

    protected string? GetItemIcon(TItem? item)
    {
        if (item.HasValue && ItemIcon != null)
        {
            return ItemIcon(item.Value);
        }

        return null;
    }

    private List<TItem?> _list = new();
    private TItem? _value;

    protected ICollection<TItem?>? InternalValues => Values?.Select(v => (TItem?)v).ToList();

    protected override void OnParametersSet()
    {
        base.OnParametersSet();
        _list = Enum.GetValues(typeof(TItem))
            .Cast<TItem?>()
            .Where(item => AllowedValues.Count == 0 || (item.HasValue && AllowedValues.Contains(item.Value)))
            .ToList();
    }

    private static string GetDisplayName(TItem? item)
    {
        if (item is null)
        {
            return string.Empty;
        }

        if (item is Enum enumVal)
        {
            return enumVal.GetDisplayName();
        }

        return item.ToString() ?? string.Empty;
    }

    private async Task OnSingleValueChanged(TItem? newValue)
    {
        _value = newValue;
        await ValueChanged.InvokeAsync(_value);
        await OnChanged.InvokeAsync(_value);
    }

    private async Task OnMultipleValuesChanged(ICollection<TItem?> newValues)
    {
        var nonNullValues = newValues
            .Where(v => v.HasValue)
            .Select(v => v!.Value)
            .ToList();
        Values = nonNullValues;
        await ValuesChanged.InvokeAsync(Values);
        await OnMultipleChanged.InvokeAsync(Values);
    }
}
