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
    [Parameter] public bool Required { get; set; }
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public bool ReadOnly { get; set; }
    [Parameter] public string Class { get; set; } = string.Empty;

    private bool _isPasswordVisible;
    private readonly string _errorId = $"input-error-{System.Guid.NewGuid():N}";

    private bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);
    private bool HasVisibleError => HasError && ShowErrorMessage;

    private string BorderAndBgClasses => HasError
        ? "border-rose-500 bg-slate-50 focus-within:border-rose-500 focus-within:bg-white focus-within:ring-3 focus-within:ring-rose-500/15"
        : "border-slate-200 bg-slate-50 focus-within:border-blue-500 focus-within:bg-white focus-within:ring-3 focus-within:ring-blue-500/15";

    private string ResolvedType => Type == "password" && _isPasswordVisible ? "text" : Type;

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

