using System;
using Microsoft.AspNetCore.Components;
using PassKee.Web.Core.Services.UI.Modal;

namespace PassKee.Web.Pages.App.Modals;

public partial class CreateVaultModal : ComponentBase
{
    [CascadingParameter] public AppModalInstance ModalInstance { get; set; } = null!;
    [Inject] public IAppModalDialogService ModalService { get; set; } = null!;

    public string Name { get; set; } = string.Empty;

    private void Cancel()
    {
        ModalService.Close(ModalInstance, AppModalResult.Cancel());
    }

    private void Save()
    {
        if (string.IsNullOrWhiteSpace(Name)) return;
        ModalService.Close(ModalInstance, AppModalResult.Ok(Name.Trim()));
    }
}
