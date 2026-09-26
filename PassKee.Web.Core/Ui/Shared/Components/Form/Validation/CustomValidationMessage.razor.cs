using System.Linq.Expressions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace PassKee.Web.Core.Ui.Shared.Components.Form.Validation;

public partial class CustomValidationMessage<TValue> : ComponentBase, IDisposable
{
    [CascadingParameter]
    private EditContext? CurrentEditContext { get; set; }

    [Parameter]
    public Expression<Func<TValue>> For { get; set; } = default!;

    private FieldIdentifier _fieldIdentifier;

    protected override void OnParametersSet()
    {
        if (CurrentEditContext != null && For != null)
        {
            _fieldIdentifier = FieldIdentifier.Create(For);
            CurrentEditContext.OnValidationStateChanged -= HandleValidationStateChanged;
            CurrentEditContext.OnValidationStateChanged += HandleValidationStateChanged;
        }
    }

    private void HandleValidationStateChanged(object? sender, ValidationStateChangedEventArgs e)
    {
        StateHasChanged();
    }

    public void Dispose()
    {
        if (CurrentEditContext != null)
        {
            CurrentEditContext.OnValidationStateChanged -= HandleValidationStateChanged;
        }
    }
}
