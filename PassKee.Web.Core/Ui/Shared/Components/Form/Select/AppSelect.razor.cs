using System.Linq.Expressions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using PassKee.Web.Core.Ui.Shared.Components.Enums;
using PassKee.Web.Core.Ui.Shared.Components.Form.Select.Core;

namespace PassKee.Web.Core.Ui.Shared.Components.Form.Select;

public partial class AppSelect<TValue> : AppBaseSelect
{
    [Parameter]
    public TValue? Value { get; set; }

    [Parameter]
    public EventCallback<TValue?> ValueChanged { get; set; }

    [Parameter]
    public EventCallback<TValue?> OnChanged { get; set; }

    [Parameter]
    public Expression<Func<TValue?>>? ValueExpression { get; set; }

    [Parameter]
    public ICollection<TValue>? Values { get; set; }

    [Parameter]
    public EventCallback<ICollection<TValue>> ValuesChanged { get; set; }

    [Parameter]
    public EventCallback<ICollection<TValue>> OnMultipleChanged { get; set; }

    [Parameter]
    public Expression<Func<ICollection<TValue>?>>? ValuesExpression { get; set; }

    [Parameter]
    public IEnumerable<TValue>? Items { get; set; }

    [Parameter]
    public Func<TValue, string>? ItemText { get; set; }

    [Parameter]
    public Func<TValue, string>? ItemDescription { get; set; }

    [Parameter]
    public Func<TValue, string>? ItemIcon { get; set; }

    [Parameter]
    public Func<TValue, bool>? ItemDisabled { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public RenderFragment<TValue>? ItemTemplate { get; set; }

    [Parameter]
    public RenderFragment? TriggerContent { get; set; }

    [Parameter]
    public bool IsAutoHeight { get; set; }

    [Parameter]
    public RenderFragment? PrefixContent { get; set; }

    [Parameter]
    public string? MaxHeight { get; set; } = "max-h-60";

    [Parameter]
    public bool HasSearch { get; set; }

    [Parameter]
    public string? SearchPlaceholder { get; set; }

    protected string _searchText = string.Empty;

    public bool HasValue => IsMultiple
        ? Values != null && Values.Count > 0
        : Value is not null;

    protected string BorderAndRingClasses
    {
        get
        {
            if (ResolvedIsInvalid)
            {
                return "border-red-500 ring-3 ring-red-500/15 text-red-900 dark:text-red-400";
            }

            if (IsOpen)
            {
                return "border-blue-500 ring-3 ring-blue-500/15 shadow-sm";
            }

            return "border-slate-200 dark:border-slate-700 hover:border-slate-300 dark:hover:border-slate-600";
        }
    }

    protected string InputSizeClasses => Size switch
    {
        ComponentSize.Small => "h-8 px-2.5 py-1 text-xs rounded-lg",
        ComponentSize.Large => "h-12 px-4 py-2.5 text-base rounded-xl",
        _ => "h-10 px-3.5 py-2 text-sm rounded-xl"
    };

    protected string AutoHeightInputSizeClasses => Size switch
    {
        ComponentSize.Small => "min-h-8 px-2.5 py-1 text-xs rounded-lg",
        ComponentSize.Large => "min-h-12 px-4 py-2.5 text-base rounded-xl",
        _ => "min-h-10 px-3.5 py-2 text-sm rounded-xl"
    };

    protected string ButtonSizeClasses => Size switch
    {
        ComponentSize.Small => "px-2 py-1 text-xs rounded-md",
        ComponentSize.Large => "px-4 py-2 text-sm rounded-lg",
        _ => "px-3 py-1.5 text-xs rounded-lg"
    };

    public string SelectedDisplaySummary
    {
        get
        {
            if (IsMultiple)
            {
                if (Values == null || Values.Count == 0)
                {
                    return !string.IsNullOrWhiteSpace(Placeholder) ? Placeholder : "Select...";
                }

                if (Values.Count == 1)
                {
                    return GetItemDisplay(Values.First());
                }

                if (Values.Count == 2)
                {
                    return $"{GetItemDisplay(Values.First())}, {GetItemDisplay(Values.Skip(1).First())}";
                }

                return $"{GetItemDisplay(Values.First())} (+{Values.Count - 1})";
            }

            if (Value is null)
            {
                return !string.IsNullOrWhiteSpace(Placeholder) ? Placeholder : "Select...";
            }

            return GetItemDisplay(Value);
        }
    }

    protected IEnumerable<TValue> FilteredItems
    {
        get
        {
            if (Items == null)
            {
                return Enumerable.Empty<TValue>();
            }

            if (string.IsNullOrWhiteSpace(_searchText))
            {
                return Items;
            }

            return Items.Where(i => GetItemDisplay(i).Contains(_searchText, StringComparison.OrdinalIgnoreCase));
        }
    }

    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        if (CurrentEditContext != null)
        {
            if (IsMultiple && ValuesExpression != null)
            {
                FieldIdentifier = FieldIdentifier.Create(ValuesExpression);
                CurrentEditContext.OnValidationStateChanged += HandleValidationStateChanged;
            }
            else if (!IsMultiple && ValueExpression != null)
            {
                FieldIdentifier = FieldIdentifier.Create(ValueExpression);
                CurrentEditContext.OnValidationStateChanged += HandleValidationStateChanged;
            }
        }
    }

    public bool IsSelected(TValue? item)
    {
        if (item is null)
        {
            return false;
        }

        if (IsMultiple)
        {
            return Values != null && Values.Contains(item);
        }

        return EqualityComparer<TValue?>.Default.Equals(Value, item);
    }

    public async Task SelectItemAsync(TValue? item)
    {
        if (IsDisabled)
        {
            return;
        }

        if (IsMultiple)
        {
            if (item is null)
            {
                return;
            }

            var currentValues = Values != null ? new List<TValue>(Values) : new List<TValue>();
            if (currentValues.Contains(item))
            {
                currentValues.Remove(item);
            }
            else
            {
                currentValues.Add(item);
            }

            Values = currentValues;
            await ValuesChanged.InvokeAsync(Values);
            await OnMultipleChanged.InvokeAsync(Values);

            if (CurrentEditContext != null && FieldIdentifier.Model != null)
            {
                CurrentEditContext.NotifyFieldChanged(FieldIdentifier);
            }

            StateHasChanged();
        }
        else
        {
            Value = item;
            await ValueChanged.InvokeAsync(Value);
            await OnChanged.InvokeAsync(Value);

            if (CurrentEditContext != null && FieldIdentifier.Model != null)
            {
                CurrentEditContext.NotifyFieldChanged(FieldIdentifier);
            }

            Close();
        }
    }

    public async Task ClearAsync()
    {
        if (IsDisabled)
        {
            return;
        }

        if (IsMultiple)
        {
            Values = new List<TValue>();
            await ValuesChanged.InvokeAsync(Values);
            await OnMultipleChanged.InvokeAsync(Values);
        }
        else
        {
            Value = default;
            await ValueChanged.InvokeAsync(Value);
            await OnChanged.InvokeAsync(Value);
        }

        if (CurrentEditContext != null && FieldIdentifier.Model != null)
        {
            CurrentEditContext.NotifyFieldChanged(FieldIdentifier);
        }

        StateHasChanged();
    }

    public string GetItemDisplay(TValue? item)
    {
        if (item is null)
        {
            return string.Empty;
        }

        if (ItemText != null)
        {
            return ItemText(item);
        }

        return item.ToString() ?? string.Empty;
    }

    protected void OnSearchInput(ChangeEventArgs e)
    {
        _searchText = e.Value?.ToString() ?? string.Empty;
    }

    public override void Close()
    {
        _searchText = string.Empty;
        base.Close();
    }
}
