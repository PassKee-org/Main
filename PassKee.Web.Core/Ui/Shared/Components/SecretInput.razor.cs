using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using PassKee.Business.Common.Utils;
using PassKee.Web.Core.Ui.Shared.Components.Enums;

namespace PassKee.Web.Core.Ui.Shared.Components;

public partial class SecretInput : ComponentBase
{
    [Inject]
    private IJSRuntime Js { get; set; } = null!;

    [Parameter]
    public string? Label { get; set; }

    [Parameter]
    public RenderFragment? LabelExtraContent { get; set; }

    [Parameter]
    public string? Value { get; set; }

    [Parameter]
    public EventCallback<string?> ValueChanged { get; set; }

    [Parameter]
    public EventCallback<ChangeEventArgs> OnInput { get; set; }

    [Parameter]
    public SecretInputType SecretType { get; set; } = SecretInputType.Password;

    [Parameter]
    public string? Placeholder { get; set; }

    [Parameter]
    public string? Icon { get; set; }

    [Parameter]
    public string? HelpText { get; set; }

    [Parameter]
    public string? ErrorMessage { get; set; }

    [Parameter]
    public string? Name { get; set; }

    [Parameter]
    public string? Autocomplete { get; set; }

    [Parameter]
    public bool Required { get; set; }

    [Parameter]
    public bool Disabled { get; set; }

    [Parameter]
    public bool ReadOnly { get; set; }

    [Parameter]
    public string Class { get; set; } = string.Empty;

    [Parameter]
    public bool ShowGenerateButton { get; set; } = true;

    [Parameter]
    public EventCallback OnGenerate { get; set; }

    [Parameter]
    public string? GenerateTooltip { get; set; }

    [Parameter]
    public bool? ShowCopyButton { get; set; }

    [Parameter]
    public EventCallback OnCopy { get; set; }

    [Parameter]
    public string CopyTooltip { get; set; } = "Copy to clipboard";

    [Parameter]
    public bool ShowVisibilityToggle { get; set; } = true;

    [Parameter]
    public bool? InitiallyMasked { get; set; }

    [Parameter]
    public bool? Monospace { get; set; }

    [Parameter]
    public RenderFragment? EndContent { get; set; }

    private bool _isMasked = true;
    private bool _isCopied;
    private bool _isInitialized;

    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        if (!_isInitialized)
        {
            _isMasked = InitiallyMasked ?? (SecretType == SecretInputType.Password);
            _isInitialized = true;
        }
    }

    private string ResolvedInputType => _isMasked ? "password" : "text";

    private string ResolvedIcon => Icon ?? (SecretType switch
    {
        SecretInputType.SecretKey => "fa-solid fa-shield-halved",
        _ => "fa-solid fa-key"
    });

    private string ResolvedPlaceholder => Placeholder ?? (SecretType switch
    {
        SecretInputType.SecretKey => "Emergency Secret Key",
        _ => "Enter password"
    });

    private string ResolvedGenerateTooltip => GenerateTooltip ?? (SecretType switch
    {
        SecretInputType.SecretKey => "Generate new Secret Key",
        _ => "Generate strong password"
    });

    private bool ResolvedShowCopyButton => ShowCopyButton ?? (SecretType == SecretInputType.SecretKey);

    private bool ResolvedMonospace => Monospace ?? (SecretType == SecretInputType.SecretKey);

    private string BorderAndBgClasses => !string.IsNullOrWhiteSpace(ErrorMessage)
        ? "border-rose-400 bg-rose-50/20 focus-within:border-rose-500 focus-within:bg-white focus-within:ring-3 focus-within:ring-rose-500/15"
        : "border-slate-200 bg-slate-50 focus-within:border-blue-500 focus-within:bg-white focus-within:ring-3 focus-within:ring-blue-500/15";

    private async Task HandleInput(ChangeEventArgs e)
    {
        Value = e.Value?.ToString();
        await ValueChanged.InvokeAsync(Value);

        if (OnInput.HasDelegate)
        {
            await OnInput.InvokeAsync(e);
        }
    }

    private async Task HandleGenerateAsync()
    {
        if (Disabled || ReadOnly)
        {
            return;
        }

        if (OnGenerate.HasDelegate)
        {
            await OnGenerate.InvokeAsync();
            return;
        }

        // Automatic generation based on SecretType
        string generatedValue = SecretType switch
        {
            SecretInputType.SecretKey => SecurityUtil.GenerateSecretKey(),
            _ => SecurityUtil.GenerateStrongPassword(16)
        };

        Value = generatedValue;
        await ValueChanged.InvokeAsync(Value);
    }

    private async Task HandleCopyAsync()
    {
        if (string.IsNullOrWhiteSpace(Value))
        {
            return;
        }

        try
        {
            await Js.InvokeVoidAsync("navigator.clipboard.writeText", Value);
            _isCopied = true;
            StateHasChanged();

            if (OnCopy.HasDelegate)
            {
                await OnCopy.InvokeAsync();
            }

            await Task.Delay(2000);
            _isCopied = false;
            StateHasChanged();
        }
        catch
        {
            // Ignore if clipboard access is denied
        }
    }

    private void ToggleMask()
    {
        _isMasked = !_isMasked;
    }
}
