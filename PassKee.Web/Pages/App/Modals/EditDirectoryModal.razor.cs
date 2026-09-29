using System;
using Microsoft.AspNetCore.Components;
using PassKee.Web.Core.Services.UI.Modal;

namespace PassKee.Web.Pages.App.Modals;

public partial class EditDirectoryModal : ComponentBase
{
    [CascadingParameter] public AppModalInstance ModalInstance { get; set; } = null!;
    [Inject] public IAppModalDialogService ModalService { get; set; } = null!;

    [Parameter] public string Name { get; set; } = string.Empty;
    [Parameter] public bool IsEdit { get; set; }

    private void Cancel()
    {
        ModalService.Close(ModalInstance, AppModalResult.Cancel());
    }

    private void Save()
    {
        if (string.IsNullOrWhiteSpace(Name)) return;
        ModalService.Close(ModalInstance, AppModalResult.Ok<string>(Name.Trim()));
    }
}
