using System.Globalization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace PassKee.Web.Core.Ui.Shared.Components.Form.TextField;

public partial class InputDateField<TValue>
{
    [Parameter]
    public Func<DateTime, bool>? DisabledDateFunc { get; set; }

    [Parameter]
    public bool IsUpdateOnInput { get; set; }

    private string _dateString = string.Empty;
    private bool _isOpen;
    private bool _isFocused;

    private bool HasEndElements =>
        EndContent != null
        || (IsClearable && !IsDisabled && !IsReadOnly && !string.IsNullOrEmpty(_dateString));

    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        if (_isFocused)
        {
            return;
        }

        if (Value is DateTime dt && dt != default)
        {
            _dateString = dt.ToString("yyyy-MM-dd");
        }
        else
        {
            _dateString = string.Empty;
        }
    }

    private void TogglePopover()
    {
        if (!IsDisabled && !IsReadOnly)
        {
            _isOpen = !_isOpen;
        }
    }

    private void HandleFocus()
    {
        _isFocused = true;
    }

    private void HandleBlur()
    {
        _isFocused = false;
    }

    private void OpenPopover()
    {
        if (!IsDisabled && !IsReadOnly && !_isOpen)
        {
            _isOpen = true;
        }
    }

    private void ClosePopover()
    {
        _isOpen = false;
    }

    private void HandleKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Escape" && _isOpen)
        {
            ClosePopover();
        }
    }

    private async Task OnCalendarDateSelected(DateTime date)
    {
        _dateString = date.ToString("yyyy-MM-dd");
        _isOpen = false;
        await UpdateValue(date);
    }

    private DateTime? GetDateTimeValue()
    {
        if (Value is DateTime dt && dt != default)
        {
            return dt;
        }
        if (DateTime.TryParseExact(_dateString, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
        {
            return parsed;
        }
        return null;
    }

    private async Task HandleInput(ChangeEventArgs e)
    {
        var raw = e.Value?.ToString() ?? string.Empty;
        _dateString = ApplyDateMask(raw);

        if (string.IsNullOrWhiteSpace(_dateString))
        {
            if (IsUpdateOnInput)
            {
                await UpdateValue(null);
            }
            return;
        }

        if (_dateString.Length == 10 && DateTime.TryParseExact(_dateString, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
        {
            if (IsUpdateOnInput)
            {
                await UpdateValue(parsedDate);
            }
        }
    }

    private async Task HandleChange(ChangeEventArgs e)
    {
        var raw = e.Value?.ToString() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(raw))
        {
            _dateString = string.Empty;
            await UpdateValue(null);
            return;
        }

        if (DateTime.TryParseExact(raw, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedExact))
        {
            _dateString = parsedExact.ToString("yyyy-MM-dd");
            await UpdateValue(parsedExact);
            return;
        }

        if (DateTime.TryParse(raw, CultureInfo.CurrentCulture, DateTimeStyles.None, out var parsedCulture))
        {
            _dateString = parsedCulture.ToString("yyyy-MM-dd");
            await UpdateValue(parsedCulture);
            return;
        }

        _dateString = ApplyDateMask(raw);
        if (_dateString.Length == 10 && DateTime.TryParseExact(_dateString, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedMasked))
        {
            await UpdateValue(parsedMasked);
        }
    }

    private async Task HandleClearAsync()
    {
        _dateString = string.Empty;
        _isOpen = false;
        await UpdateValue(null);
        await ClearAsync();
    }

    private async Task UpdateValue(DateTime? dt)
    {
        if (typeof(TValue) == typeof(DateTime))
        {
            await SetValueAsync((TValue)(object)(dt ?? default(DateTime)));
        }
        else
        {
            await SetValueAsync((TValue?)(object?)dt);
        }
    }

    private static string ApplyDateMask(string input)
    {
        var digits = new string(input.Where(char.IsDigit).ToArray());

        if (digits.Length == 0)
        {
            return string.Empty;
        }

        if (digits.Length <= 4)
        {
            return digits;
        }

        if (digits.Length <= 6)
        {
            return $"{digits.Substring(0, 4)}-{digits.Substring(4)}";
        }

        var year = digits.Substring(0, 4);
        var month = digits.Substring(4, 2);
        var day = digits.Substring(6, Math.Min(2, digits.Length - 6));

        return $"{year}-{month}-{day}";
    }
}
