using Microsoft.AspNetCore.Components;
using PassKee.Web.Core.Ui.Shared.Components.Enums;

namespace PassKee.Web.Core.Ui.Shared.Components.Form.TextField;

public partial class InputTextareaField
{
    [Parameter]
    public string? Id { get; set; }

    [Parameter]
    public bool BindOnInput { get; set; }

    [Parameter]
    public int Rows { get; set; } = 4;

    [Parameter]
    public int? MaxLength { get; set; }

    [Parameter]
    public int MaxRows { get; set; } = 8;

    [Parameter]
    public int MinRows { get; set; } = 3;

    private string ComputedTextareaClass
    {
        get
        {
            var sizeClass = Size == ComponentSize.Small ? "text-xs px-2.5 py-1.5 rounded-lg" : "text-sm px-3.5 py-2.5 rounded-xl";
            var baseClass = $"w-full border transition-all duration-150 focus:outline-hidden disabled:cursor-not-allowed disabled:bg-slate-50 dark:disabled:bg-slate-900/50 disabled:text-slate-400 {sizeClass} {Class}".Trim();

            if (HasError)
            {
                return $"{baseClass} border-rose-500 ring-3 ring-rose-500/15 text-rose-900 dark:text-rose-300 bg-white dark:bg-slate-800 placeholder:text-rose-400";
            }

            if (IsFlat)
            {
                return $"{baseClass} border-transparent bg-slate-100 dark:bg-slate-800 text-slate-800 dark:text-slate-100 placeholder:text-slate-400 hover:bg-slate-200/70 focus:bg-white dark:focus:bg-slate-800 focus:border-blue-500 focus:ring-3 focus:ring-blue-500/15";
            }

            return $"{baseClass} border-slate-200 dark:border-slate-700 bg-white dark:bg-slate-800 text-slate-800 dark:text-slate-100 placeholder:text-slate-400 hover:border-slate-300 dark:hover:border-slate-600 focus:border-blue-500 focus:ring-3 focus:ring-blue-500/15 shadow-2xs";
        }
    }

    private bool _isFocused;
    private string _textValue = string.Empty;
    private string? _lastValue;

    protected override void OnParametersSet()
    {
        base.OnParametersSet();
        if (!_isFocused || _lastValue != Value)
        {
            _textValue = Value ?? string.Empty;
            _lastValue = Value;
        }
    }

    private async Task OnInputChanged(ChangeEventArgs e)
    {
        _textValue = e.Value?.ToString() ?? string.Empty;
        if (BindOnInput)
        {
            await SetValueAsync(_textValue);
        }
    }

    private async Task OnValueChanged(ChangeEventArgs e)
    {
        _textValue = e.Value?.ToString() ?? string.Empty;
        if (!BindOnInput)
        {
            await SetValueAsync(_textValue);
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
}
