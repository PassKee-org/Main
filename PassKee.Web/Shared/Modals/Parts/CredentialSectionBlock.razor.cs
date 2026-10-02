using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using PassKee.Api.Shared.Models.Vaults.Payloads;
using PassKee.Web.Core.Services.UI.Modal;

namespace PassKee.Web.Shared.Modals.Parts;

public partial class CredentialSectionBlock : ComponentBase
{
    [Inject] private IAppModalDialogService ModalService { get; set; } = null!;

    [Parameter, EditorRequired] public CredentialSection Section { get; set; } = null!;
    [Parameter] public bool IsEditing { get; set; }
    [Parameter] public EventCallback OnRemove { get; set; }

    private void AddField(CredentialFieldType type)
    {
        Section.Fields.Add(new CredentialField
        {
            Type = type,
            Label = CredentialFieldMenuBlock.GetFieldLabel(type)
        });
    }

    private async Task ConfirmAndRemoveAsync()
    {
        var title = string.IsNullOrWhiteSpace(Section.Title) ? "this section" : $"'{Section.Title}'";
        var confirmed = await ModalService.ShowConfirmationAsync(
            $"Are you sure you want to delete {title} and all its fields?",
            title: "Delete Section",
            confirmText: "Delete",
            cancelText: "Cancel",
            type: AppConfirmationType.Danger);

        if (confirmed)
        {
            await OnRemove.InvokeAsync();
        }
    }
}
