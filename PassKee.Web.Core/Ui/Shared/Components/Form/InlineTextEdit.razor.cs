using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace PassKee.Web.Core.Ui.Shared.Components.Form;

public partial class InlineTextEdit : ComponentBase
{
    [Parameter]
    public required string Value { get; set; }

    [Parameter]
    public EventCallback<string> OnSave { get; set; }

    [Parameter]
    public string? Placeholder { get; set; }

    [Parameter]
    public bool Multiline { get; set; }

    [Parameter]
    public bool Disabled { get; set; }

    [Parameter]
    public bool SaveOnBlur { get; set; }

    [Parameter]
    public int HeadingLevel { get; set; }

    [Parameter]
    public string? Class { get; set; }

    [Parameter]
    public string ViewClassName { get; set; } = string.Empty;

    [Parameter]
    public string InputClassName { get; set; } = string.Empty;

    [Parameter]
    public Func<string, string?>? Validate { get; set; }

    private ElementReference _editor;
    private string _draft = string.Empty;
    private string? _error;
    private bool _isEditing;
    private bool _isSaving;
    private bool _shouldFocus;
    private bool IsDirty => !string.Equals(_draft, Value, StringComparison.Ordinal);
    private string DisplayValue => string.IsNullOrWhiteSpace(Value) ? Placeholder ?? string.Empty : Value;

    protected override void OnParametersSet()
    {
        if (!_isEditing)
        {
            _draft = Value;
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (_shouldFocus)
        {
            await _editor.FocusAsync();
            _shouldFocus = false;
        }
    }

    private void BeginEditing()
    {
        if (Disabled)
        {
            return;
        }

        _draft = Value;
        _error = null;
        _isEditing = true;
        _shouldFocus = true;
    }

    private async Task SaveAsync()
    {
        if (_isSaving)
        {
            return;
        }

        var nextValue = _draft.Trim();
        _error = Validate?.Invoke(nextValue);
        if (string.IsNullOrWhiteSpace(_error) && string.IsNullOrWhiteSpace(nextValue))
        {
            _error = "This field is required.";
        }
        if (!string.IsNullOrWhiteSpace(_error))
        {
            return;
        }

        if (!IsDirty)
        {
            _isEditing = false;
            return;
        }

        _isSaving = true;
        try
        {
            await OnSave.InvokeAsync(nextValue);
            _isEditing = false;
        }
        catch (Exception exception)
        {
            _error = exception.Message;
        }
        finally
        {
            _isSaving = false;
        }
    }

    private void Cancel()
    {
        if (_isSaving)
        {
            return;
        }

        _draft = Value;
        _error = null;
        _isEditing = false;
    }

    private Task OnKeyDown(KeyboardEventArgs eventArguments)
    {
        if (eventArguments.Key == "Escape")
        {
            Cancel();
        }
        else if (!Multiline && eventArguments.Key == "Enter")
        {
            return SaveAsync();
        }

        return Task.CompletedTask;
    }

    private Task OnBlur()
    {
        return SaveOnBlur && IsDirty ? SaveAsync() : Task.CompletedTask;
    }
}
