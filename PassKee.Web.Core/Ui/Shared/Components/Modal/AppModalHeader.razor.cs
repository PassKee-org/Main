using Microsoft.AspNetCore.Components;
using PassKee.Web.Core.Services.UI.Modal;
using PassKee.Web.Core.Ui.Shared.Components.Enums;

namespace PassKee.Web.Core.Ui.Shared.Components.Modal;

public partial class AppModalHeader : ComponentBase
{
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public string? Icon { get; set; }

    [Parameter]
    public string? IconClass
    {
        get => Icon;
        set => Icon = value;
    }

    [Parameter]
    public ComponentColor Color { get; set; } = ComponentColor.Primary;

    [Parameter]
    public ComponentColor IconColor
    {
        get => Color;
        set => Color = value;
    }

    [Parameter]
    public string? IconColorClass { get; set; }

    [Parameter]
    public RenderFragment? IconContent { get; set; }

    private string ComputedIconBoxClass
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(IconColorClass))
            {
                return IconColorClass;
            }

            return Color switch
            {
                ComponentColor.Primary => "bg-blue-50 text-blue-600",
                ComponentColor.Success => "bg-emerald-50 text-emerald-600",
                ComponentColor.Warning => "bg-amber-50 text-amber-600",
                ComponentColor.Danger => "bg-rose-50 text-rose-600",
                ComponentColor.Secondary => "bg-slate-100 text-slate-700",
                ComponentColor.Info => "bg-sky-50 text-sky-600",
                ComponentColor.Default => "bg-slate-100 text-slate-700",
                _ => "bg-blue-50 text-blue-600"
            };
        }
    }

    [Parameter]
    public string? Title { get; set; }

    [Parameter]
    public string? Description { get; set; }

    [Parameter]
    public string? Subtitle
    {
        get => Description;
        set => Description = value;
    }

    [Parameter]
    public bool HasCloseButton { get; set; } = true;

    [Parameter]
    public string? Class { get; set; }

    [Parameter]
    public EventCallback OnClose { get; set; }

    [CascadingParameter]
    public AppModalInstance? ModalInstance { get; set; }

    [CascadingParameter]
    public AppModal? DeclarativeModal { get; set; }

    private bool IsCloseButtonVisible => HasCloseButton
        && (ModalInstance == null || ModalInstance.Options.HasCloseButton)
        && (DeclarativeModal == null || DeclarativeModal.HasCloseButton);

    private async Task OnCloseClick()
    {
        if (OnClose.HasDelegate)
        {
            await OnClose.InvokeAsync();
        }
        if (DeclarativeModal != null)
        {
            await DeclarativeModal.CloseAsync();
        }
        ModalInstance?.Close(AppModalResult.Cancel("close_button"));
    }
}
