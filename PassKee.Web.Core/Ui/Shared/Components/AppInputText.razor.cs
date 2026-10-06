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
    [Parameter] public Enums.InputVariant Variant { get; set; } = Enums.InputVariant.Outline;
    [Parameter] public string InputClass { get; set; } = string.Empty;
    [Parameter] public string? AriaLabel { get; set; }
    [Parameter] public bool Multiline { get; set; }
    [Parameter] public int Rows { get; set; } = 3;
    [Parameter] public bool ShowPasswordToggle { get; set; } = true;
    [Parameter] public bool? IsPasswordVisible { get; set; }
    [Parameter] public EventCallback<bool> IsPasswordVisibleChanged { get; set; }

    private bool _internalPasswordVisible;
    private readonly string _inputId = $"input-{System.Guid.NewGuid():N}";
    private readonly string _errorId = $"input-error-{System.Guid.NewGuid():N}";

    private bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);
    private bool HasVisibleError => HasError && ShowErrorMessage;

    private bool IsPasswordMode => Type == "password";
    private bool EffectivePasswordVisible => IsPasswordVisible ?? _internalPasswordVisible;

    private string BorderAndBgClasses => HasError
        ? "border-rose-500 bg-slate-50 focus-within:border-rose-500 focus-within:bg-white focus-within:ring-3 focus-within:ring-rose-500/15"
        : "border-slate-200 bg-slate-50 focus-within:border-blue-500 focus-within:bg-white focus-within:ring-3 focus-within:ring-blue-500/15";

    private string ContainerClasses => Variant == Enums.InputVariant.Plain
        ? "relative flex items-center transition min-w-0 w-full"
        : $"relative flex items-center rounded-xl border transition {BorderAndBgClasses} {(Disabled ? "opacity-60 cursor-not-allowed bg-slate-100" : "")}";

    private string ComputedInputClass
    {
        get
        {
            var baseClass = Variant == Enums.InputVariant.Plain
                ? "w-full bg-transparent border-0 p-0 focus:outline-none focus:ring-0 disabled:cursor-not-allowed"
                : $"w-full bg-transparent border-0 px-3.5 py-2.5 text-base sm:text-sm text-slate-900 placeholder:text-slate-400 focus:outline-none focus:ring-0 disabled:cursor-not-allowed {(!string.IsNullOrWhiteSpace(Icon) ? "!pl-1.5" : "")}";

            return string.IsNullOrWhiteSpace(InputClass) ? baseClass : $"{baseClass} {InputClass}";
        }
    }

    private string ResolvedType =>
        IsPasswordMode
            ? (PreventPasswordManager ? "text" : (EffectivePasswordVisible ? "text" : "password"))
            : Type;

    private string? ResolvedStyle
    {
        get
        {
            var securityStyle = IsPasswordMode && PreventPasswordManager && !EffectivePasswordVisible
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

    private async Task TogglePasswordVisibility()
    {
        if (IsPasswordVisible.HasValue)
        {
            await IsPasswordVisibleChanged.InvokeAsync(!IsPasswordVisible.Value);
        }
        else
        {
            _internalPasswordVisible = !_internalPasswordVisible;
        }
    }
}

