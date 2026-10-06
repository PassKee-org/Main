using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace PassKee.Web.Core.Ui.Shared.Components;

public partial class AppInputText : ComponentBase
{
    [Parameter] public string? Label { get; set; }
    [Parameter] public string? Value { get; set; }
    [Parameter] public EventCallback<string?> ValueChanged { get; set; }
    [Parameter] public string Type { get; set; } = "text";
    [Parameter] public string Placeholder { get; set; } = string.Empty;
    [Parameter] public string? Icon { get; set; }
    [Parameter] public string? HelpText { get; set; }
    [Parameter] public string? ErrorMessage { get; set; }
    [Parameter] public bool ShowErrorMessage { get; set; } = true;
    [Parameter] public string? Name { get; set; }
    [Parameter] public string? Autocomplete { get; set; }
    [Parameter] public string? Style { get; set; }
    [Parameter] public bool Required { get; set; }
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public bool ReadOnly { get; set; }
    [Parameter] public string Class { get; set; } = string.Empty;
    [Parameter] public bool PreventPasswordManager { get; set; } = true;

    private bool _isPasswordVisible;
    private readonly string _inputId = $"input-{System.Guid.NewGuid():N}";
    private readonly string _errorId = $"input-error-{System.Guid.NewGuid():N}";

    private bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);
    private bool HasVisibleError => HasError && ShowErrorMessage;

    private bool IsPasswordMode => Type == "password";

    private string BorderAndBgClasses => HasError
        ? "border-rose-500 bg-slate-50 focus-within:border-rose-500 focus-within:bg-white focus-within:ring-3 focus-within:ring-rose-500/15"
        : "border-slate-200 bg-slate-50 focus-within:border-blue-500 focus-within:bg-white focus-within:ring-3 focus-within:ring-blue-500/15";

    private string ResolvedType =>
        IsPasswordMode
            ? (PreventPasswordManager ? "text" : (_isPasswordVisible ? "text" : "password"))
            : Type;

    private string? ResolvedStyle
    {
        get
        {
            var securityStyle = IsPasswordMode && PreventPasswordManager && !_isPasswordVisible
                ? "-webkit-text-security: disc; text-security: disc;"
                : null;

            if (securityStyle != null && !string.IsNullOrWhiteSpace(Style))
            {
                return $"{securityStyle} {Style}";
            }

            return securityStyle ?? Style;
        }
    }

    private string? ResolvedAutocomplete =>
        !string.IsNullOrWhiteSpace(Autocomplete)
            ? Autocomplete
            : (IsPasswordMode && PreventPasswordManager ? "off" : null);

    private async Task HandleInput(ChangeEventArgs e)
    {
        Value = e.Value?.ToString();
        await ValueChanged.InvokeAsync(Value);
    }

    private void TogglePasswordVisibility()
    {
        _isPasswordVisible = !_isPasswordVisible;
    }
}

