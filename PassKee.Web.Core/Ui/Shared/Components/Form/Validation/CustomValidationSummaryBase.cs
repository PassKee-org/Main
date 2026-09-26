using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace PassKee.Web.Core.Ui.Shared.Components.Form.Validation;

public class CustomValidationSummaryBase : ComponentBase, IDisposable
{
    [CascadingParameter]
    protected EditContext? CurrentEditContext { get; set; }

    [Parameter]
    public bool HasInlineValidation { get; set; } = true;

    [Parameter]
    public bool IsInlineValidationPresent
    {
        get => HasInlineValidation;
        set => HasInlineValidation = value;
    }

    protected ICollection<string> _validationMessages = new List<string>();

    protected override void OnInitialized()
    {
        if (CurrentEditContext == null)
        {
            throw new InvalidOperationException($"{nameof(CustomValidationSummaryBase)} requires a cascading parameter of type {nameof(EditContext)}. For example, you can use {nameof(CustomValidationSummaryBase)} inside an {nameof(EditForm)}.");
        }

        CurrentEditContext.OnValidationStateChanged += HandleValidationStateChanged;
        UpdateValidationMessages();
    }

    private void HandleValidationStateChanged(object? sender, ValidationStateChangedEventArgs e)
    {
        UpdateValidationMessages();
        StateHasChanged();
    }

    private void UpdateValidationMessages()
    {
        if (CurrentEditContext == null)
        {
            _validationMessages = new List<string>();
            return;
        }

        if (HasInlineValidation && CurrentEditContext.Model != null)
        {
            var modelProperties = CurrentEditContext.Model.GetType()
                .GetProperties()
                .Select(p => p.Name)
                .ToHashSet();

            _validationMessages = CurrentEditContext.GetValidationMessages()
                .Where(msg =>
                {
                    var isFieldMessage = modelProperties.Any(prop =>
                        CurrentEditContext.GetValidationMessages(new FieldIdentifier(CurrentEditContext.Model, prop)).Contains(msg));
                    return !isFieldMessage;
                })
                .ToList();
        }
        else
        {
            _validationMessages = CurrentEditContext.GetValidationMessages().ToList();
        }
    }

    public void Dispose()
    {
        if (CurrentEditContext != null)
        {
            CurrentEditContext.OnValidationStateChanged -= HandleValidationStateChanged;
        }
    }
}
