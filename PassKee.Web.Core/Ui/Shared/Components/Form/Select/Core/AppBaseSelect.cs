using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using PassKee.Web.Core.Ui.Shared.Components.Enums;

namespace PassKee.Web.Core.Ui.Shared.Components.Form.Select.Core;

public abstract class AppBaseSelect : ComponentBase, IDisposable
{
    [CascadingParameter]
    protected EditContext? CurrentEditContext { get; set; }

    [Parameter]
    public string? Label { get; set; }

    [Parameter]
    public string Placeholder { get; set; } = string.Empty;

    [Parameter]
    public ComponentSize Size { get; set; } = ComponentSize.Medium;

    [Parameter]
    public bool IsDisabled { get; set; }

    [Parameter]
    public bool Disabled
    {
        get => IsDisabled;
        set => IsDisabled = value;
    }

    [Parameter]
    public bool IsClearable { get; set; }

    [Parameter]
    public bool Clearable
    {
        get => IsClearable;
        set => IsClearable = value;
    }

    [Parameter]
    public bool IsFullWidth { get; set; } = true;

    [Parameter]
    public bool FullWidth
    {
        get => IsFullWidth;
        set => IsFullWidth = value;
    }

    [Parameter]
    public bool IsMultiple { get; set; }

    [Parameter]
    public bool IsRequired { get; set; }

    [Parameter]
    public bool Required
    {
        get => IsRequired;
        set => IsRequired = value;
    }

    [Parameter]
    public SelectVariant Variant { get; set; } = SelectVariant.Input;

    [Parameter]
    public DropDownType? SelectType { get; set; }

    [Parameter]
    public ComponentColor Color { get; set; } = ComponentColor.Default;

    [Parameter]
    public ComponentColor DropDownColor
    {
        get => Color;
        set => Color = value;
    }

    [Parameter]
    public string? ButtonColorClass { get; set; }

    [Parameter]
    public string? Class { get; set; }

    [Parameter]
    public string? PopupClass { get; set; }

    public bool IsOpen { get; protected set; }

    protected virtual string ButtonColorClasses
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(ButtonColorClass))
            {
                return ButtonColorClass;
            }

            return Color switch
            {
                ComponentColor.Primary => "bg-blue-50 text-blue-700 border-blue-200 hover:bg-blue-100 dark:bg-blue-950/40 dark:text-blue-300 dark:border-blue-800",
                ComponentColor.Success => "bg-emerald-50 text-emerald-700 border-emerald-200 hover:bg-emerald-100 dark:bg-emerald-950/40 dark:text-emerald-300 dark:border-emerald-800",
                ComponentColor.Warning => "bg-amber-50 text-amber-700 border-amber-200 hover:bg-amber-100 dark:bg-amber-950/40 dark:text-amber-300 dark:border-amber-800",
                ComponentColor.Danger => "bg-rose-50 text-rose-700 border-rose-200 hover:bg-rose-100 dark:bg-rose-950/40 dark:text-rose-300 dark:border-rose-800",
                ComponentColor.Secondary => "bg-slate-100 text-slate-700 border-slate-200 hover:bg-slate-200 dark:bg-slate-800 dark:text-slate-300 dark:border-slate-700",
                ComponentColor.Info => "bg-sky-50 text-sky-700 border-sky-200 hover:bg-sky-100 dark:bg-sky-950/40 dark:text-sky-300 dark:border-sky-800",
                _ => "bg-slate-100 text-slate-700 border-slate-200 hover:bg-slate-200 dark:bg-slate-800 dark:text-slate-300 dark:border-slate-700"
            };
        }
    }

    protected FieldIdentifier FieldIdentifier;

    [Parameter]
    public bool? IsInvalid { get; set; }

    protected bool ResolvedIsInvalid => IsInvalid ?? (CurrentEditContext != null
                                && FieldIdentifier.Model != null
                                && CurrentEditContext.GetValidationMessages(FieldIdentifier).Any());

    protected SelectVariant ResolvedVariant => SelectType switch
    {
        DropDownType.DropDown => SelectVariant.Button,
        DropDownType.Select => SelectVariant.Input,
        _ => Variant
    };

    public virtual void ToggleOpen()
    {
        if (IsDisabled)
        {
            return;
        }

        IsOpen = !IsOpen;
        StateHasChanged();
    }

    public virtual void Open()
    {
        if (IsDisabled || IsOpen)
        {
            return;
        }

        IsOpen = true;
        StateHasChanged();
    }

    public virtual void Close()
    {
        if (!IsOpen)
        {
            return;
        }

        IsOpen = false;
        StateHasChanged();
    }

    protected virtual void HandleKeyDown(KeyboardEventArgs e)
    {
        if (IsDisabled)
        {
            return;
        }

        if (e.Key == "Escape" && IsOpen)
        {
            Close();
        }
        else if ((e.Key == "Enter" || e.Key == " " || e.Key == "ArrowDown") && !IsOpen)
        {
            Open();
        }
    }

    protected virtual void HandleValidationStateChanged(object? sender, ValidationStateChangedEventArgs e)
    {
        StateHasChanged();
    }

    public virtual void Dispose()
    {
        if (CurrentEditContext != null)
        {
            CurrentEditContext.OnValidationStateChanged -= HandleValidationStateChanged;
        }
    }
}
