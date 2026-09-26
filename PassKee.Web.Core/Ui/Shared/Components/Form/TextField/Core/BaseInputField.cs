using System.Linq.Expressions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using PassKee.Web.Core.Ui.Shared.Components.Enums;

namespace PassKee.Web.Core.Ui.Shared.Components.Form.TextField.Core;

public abstract class BaseInputField<TValue> : ComponentBase, IDisposable
{
    [CascadingParameter]
    protected EditContext? CurrentEditContext { get; set; }

    [Parameter]
    public TValue? Value { get; set; }

    [Parameter]
    public EventCallback<TValue?> ValueChanged { get; set; }

    [Parameter]
    public EventCallback<TValue?> OnChanged { get; set; }

    [Parameter]
    public Expression<Func<TValue?>>? ValueExpression { get; set; }

    [Parameter]
    public string? Label { get; set; }

    [Parameter]
    public string Placeholder { get; set; } = string.Empty;

    [Parameter]
    public string Description { get; set; } = string.Empty;

    [Parameter]
    public bool Disabled { get; set; }

    [Parameter]
    public bool IsDisabled
    {
        get => Disabled;
        set => Disabled = value;
    }

    [Parameter]
    public bool Required { get; set; }

    [Parameter]
    public bool IsRequired
    {
        get => Required;
        set => Required = value;
    }

    [Parameter]
    public bool ReadOnly { get; set; }

    [Parameter]
    public bool IsReadOnly
    {
        get => ReadOnly;
        set => ReadOnly = value;
    }

    [Parameter]
    public bool FullWidth { get; set; } = true;

    [Parameter]
    public bool IsFullWidth
    {
        get => FullWidth;
        set => FullWidth = value;
    }

    [Parameter]
    public bool Clearable { get; set; }

    [Parameter]
    public bool IsClearable
    {
        get => Clearable;
        set => Clearable = value;
    }

    [Parameter]
    public bool IsFlat { get; set; }

    [Parameter]
    public bool Flat
    {
        get => IsFlat;
        set => IsFlat = value;
    }

    [Parameter]
    public ComponentSize Size { get; set; } = ComponentSize.Medium;

    [Parameter]
    public string Class { get; set; } = string.Empty;

    [Parameter]
    public string WrapperClass { get; set; } = string.Empty;

    [Parameter]
    public RenderFragment? StartContent { get; set; }

    [Parameter]
    public RenderFragment? EndContent { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    protected FieldIdentifier FieldIdentifier;

    public bool HasError => CurrentEditContext != null
                            && FieldIdentifier.Model != null
                            && CurrentEditContext.GetValidationMessages(FieldIdentifier).Any();

    public string FirstError => CurrentEditContext != null && FieldIdentifier.Model != null
        ? CurrentEditContext.GetValidationMessages(FieldIdentifier).FirstOrDefault() ?? string.Empty
        : string.Empty;

    public virtual bool HasValue => Value is not null && !string.IsNullOrWhiteSpace(Value.ToString());

    protected virtual string InputSizeClasses => Size switch
    {
        ComponentSize.Small => "h-8 text-xs rounded-lg",
        ComponentSize.Large => "h-12 text-base rounded-xl",
        _ => "h-10 text-sm rounded-xl"
    };

    protected virtual string ContainerClasses
    {
        get
        {
            var baseClasses = "relative w-full flex items-center transition-all duration-150 border";

            if (HasError)
            {
                return $"{baseClasses} border-rose-500 ring-3 ring-rose-500/15 text-rose-900 dark:text-rose-300 bg-white dark:bg-slate-800";
            }

            if (IsDisabled)
            {
                return $"{baseClasses} border-slate-200 dark:border-slate-700 bg-slate-50 dark:bg-slate-900/50 text-slate-400 cursor-not-allowed opacity-75";
            }

            if (IsFlat)
            {
                return $"{baseClasses} border-transparent bg-slate-100 dark:bg-slate-800 text-slate-800 dark:text-slate-100 hover:bg-slate-200/70 focus-within:bg-white dark:focus-within:bg-slate-800 focus-within:border-blue-500 focus-within:ring-3 focus-within:ring-blue-500/15";
            }

            return $"{baseClasses} border-slate-200 dark:border-slate-700 bg-white dark:bg-slate-800 text-slate-800 dark:text-slate-100 hover:border-slate-300 dark:hover:border-slate-600 focus-within:border-blue-500 focus-within:ring-3 focus-within:ring-blue-500/15 shadow-2xs";
        }
    }

    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        if (CurrentEditContext != null && ValueExpression != null)
        {
            FieldIdentifier = FieldIdentifier.Create(ValueExpression);
            CurrentEditContext.OnValidationStateChanged -= HandleValidationStateChanged;
            CurrentEditContext.OnValidationStateChanged += HandleValidationStateChanged;
        }
    }

    protected virtual void HandleValidationStateChanged(object? sender, ValidationStateChangedEventArgs e)
    {
        StateHasChanged();
    }

    public virtual async Task SetValueAsync(TValue? newValue)
    {
        if (IsDisabled || IsReadOnly)
        {
            return;
        }

        if (EqualityComparer<TValue>.Default.Equals(Value, newValue))
        {
            return;
        }

        Value = newValue;
        await ValueChanged.InvokeAsync(Value);
        await OnChanged.InvokeAsync(Value);

        if (CurrentEditContext != null && FieldIdentifier.Model != null)
        {
            CurrentEditContext.NotifyFieldChanged(FieldIdentifier);
        }

        StateHasChanged();
    }

    public virtual async Task ClearAsync()
    {
        if (IsDisabled || IsReadOnly)
        {
            return;
        }

        await SetValueAsync(default);
    }

    public virtual void Dispose()
    {
        if (CurrentEditContext != null)
        {
            CurrentEditContext.OnValidationStateChanged -= HandleValidationStateChanged;
        }
    }
}
