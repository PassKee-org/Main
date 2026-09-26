using System.ComponentModel;
using System.Globalization;
using Microsoft.AspNetCore.Components;

namespace PassKee.Web.Core.Ui.Shared.Components.Form.TextField;

public partial class InputNumericField<TItem> where TItem : struct
{
    [Parameter]
    public string? Step { get; set; }

    [Parameter]
    public string? Min { get; set; }

    [Parameter]
    public string? Max { get; set; }

    [Parameter]
    public bool IsUpdateOnInput { get; set; }

    private string _stringValue = string.Empty;
    private bool _isFocused;

    private bool HasEndElements =>
        EndContent != null
        || (IsClearable && !IsDisabled && !IsReadOnly && !string.IsNullOrEmpty(_stringValue));

    protected override void OnParametersSet()
    {
        base.OnParametersSet();
        if (!_isFocused)
        {
            _stringValue = Value.ToString() ?? string.Empty;
        }
    }

    private async Task HandleInput(ChangeEventArgs e)
    {
        _stringValue = e.Value?.ToString() ?? string.Empty;
        if (IsUpdateOnInput && TryParse(_stringValue, out var parsed))
        {
            await SetValueAsync(parsed);
        }
    }

    private void HandleFocus() => _isFocused = true;

    private void HandleBlur() => _isFocused = false;

    private async Task HandleChange(ChangeEventArgs e)
    {
        _stringValue = e.Value?.ToString() ?? string.Empty;
        if (TryParse(_stringValue, out var parsed))
        {
            await SetValueAsync(parsed);
        }
        else if (string.IsNullOrWhiteSpace(_stringValue))
        {
            await SetValueAsync(default);
        }
    }

    public override async Task ClearAsync()
    {
        _stringValue = string.Empty;
        await base.ClearAsync();
    }

    private static bool TryParse(string? input, out TItem value)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                value = default;
                return true;
            }

            var targetType = Nullable.GetUnderlyingType(typeof(TItem)) ?? typeof(TItem);
            var converter = TypeDescriptor.GetConverter(targetType);
            var converted = converter.ConvertFromString(null, CultureInfo.InvariantCulture, input!);
            if (converted != null)
            {
                value = (TItem)converted;
                return true;
            }

            value = default;
            return false;
        }
        catch
        {
            try
            {
                var targetType = Nullable.GetUnderlyingType(typeof(TItem)) ?? typeof(TItem);
                var converter = TypeDescriptor.GetConverter(targetType);
                var converted = converter.ConvertFromString(null, CultureInfo.CurrentCulture, input!);
                if (converted != null)
                {
                    value = (TItem)converted;
                    return true;
                }
            }
            catch
            {
            }

            value = default;
            return false;
        }
    }
}
