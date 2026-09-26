using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using PassKee.Web.Core.Ui.Shared.Components.Form.TextField.Models;

namespace PassKee.Web.Core.Ui.Shared.Components.Form.TextField;

public partial class InputTextField
{
    [Parameter]
    public InputType Type { get; set; } = InputType.Text;

    [Parameter]
    public bool IsShowValidationState { get; set; }

    [Parameter]
    public bool ShowValidationState
    {
        get => IsShowValidationState;
        set => IsShowValidationState = value;
    }

    [Parameter]
    public bool IsUpdateOnInput { get; set; }

    [Parameter]
    public int? MaxLength { get; set; }

    [Parameter]
    public EventCallback<FocusEventArgs> OnBlur { get; set; }

    [Parameter]
    public EventCallback<ChangeEventArgs> OnChange { get; set; }

    private bool _isPasswordVisible;
    private bool _isFocused;
    private string _inputValue = string.Empty;
    private string? _lastValue;

    protected override void OnParametersSet()
    {
        base.OnParametersSet();
        if (!_isFocused || _lastValue != Value)
        {
            _inputValue = Value ?? string.Empty;
            _lastValue = Value;
        }
    }

    private bool IsSuccess => !HasError && !string.IsNullOrWhiteSpace(Value);

    private bool HasEndElements =>
        EndContent != null
        || (IsShowValidationState && IsSuccess)
        || (Type == InputType.Password)
        || (IsClearable && !IsDisabled && !IsReadOnly && !string.IsNullOrEmpty(Value));

    private string ResolvedHtmlInputType => Type switch
    {
        InputType.Password => _isPasswordVisible ? "text" : "password",
        InputType.Email => "email",
        InputType.Url => "url",
        InputType.Number => "number",
        InputType.Tel => "tel",
        InputType.Search => "search",
        _ => "text"
    };

    private async Task HandleInput(ChangeEventArgs e)
    {
        _inputValue = e.Value?.ToString() ?? string.Empty;
        if (IsUpdateOnInput)
        {
            await SetValueAsync(_inputValue);
        }
    }

    private async Task HandleChange(ChangeEventArgs e)
    {
        _inputValue = e.Value?.ToString() ?? string.Empty;
        if (!IsUpdateOnInput)
        {
            await SetValueAsync(_inputValue);
        }

        if (OnChange.HasDelegate)
        {
            await OnChange.InvokeAsync(e);
        }
    }

    private void HandleFocus()
    {
        _isFocused = true;
    }

    private async Task HandleBlur(FocusEventArgs e)
    {
        _isFocused = false;
        if (OnBlur.HasDelegate)
        {
            await OnBlur.InvokeAsync(e);
        }
    }

    private void TogglePasswordVisibility()
    {
        _isPasswordVisible = !_isPasswordVisible;
    }
}
