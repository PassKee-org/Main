using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fluxor;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using PassKee.Web.Components;
using PassKee.Web.Models.Vaults;
using PassKee.Web.Pages.App.Modals;
using PassKee.Web.Store.Vaults;

namespace PassKee.Web.Pages.App;

public partial class VaultSidebarBlock : BaseReactiveComponent
{
    [Inject] public IState<VaultsState> VaultsState { get; set; } = null!;

    [Parameter]
    public EventCallback OnItemNavigated { get; set; }

    private IEnumerable<Guid?> VaultIds => VaultsState.Value.Vaults.Select(vault => (Guid?)vault.Id);

    private string GetVaultName(Guid? vaultId) =>
        VaultsState.Value.Vaults.FirstOrDefault(vault => vault.Id == vaultId)?.Name ?? string.Empty;

    private async Task OnSelectVault(Guid? vaultId)
    {
        if (vaultId.HasValue)
        {
            Dispatcher.Dispatch(new SelectVaultAction(vaultId.Value));
            Dispatcher.Dispatch(new LoadVaultDetailsAction(vaultId.Value));
            await OnItemNavigated.InvokeAsync();
        }
    }

    private async Task OpenCreateVaultModal()
    {
        var result = await ModalService.ShowAsync<CreateVaultModal>();
        if (result.IsSuccess && result.Data != null)
        {
            var vaultName = (string)result.Data;
            if (!string.IsNullOrWhiteSpace(vaultName))
            {
                Dispatcher.Dispatch(new CreateVaultAction(vaultName.Trim(), string.Empty));
            }
        }
    }

    private async Task SelectDirectory(Guid? id)
    {
        Dispatcher.Dispatch(new SelectDirectoryAction(id));
        await OnItemNavigated.InvokeAsync();
    }

    private Task OpenCreateSubDirectoryModal(DecryptedDirectory parentDir) =>
        OpenDirectoryModal(null, parentDir.Id);

    private Task OpenDirectoryModal(DecryptedDirectory? dir) =>
        OpenDirectoryModal(dir, null);

    private async Task OpenDirectoryModal(DecryptedDirectory? dir, Guid? parentDirectoryId)
    {
        if (!VaultsState.Value.ActiveVaultId.HasValue) return;
        var vaultId = VaultsState.Value.ActiveVaultId.Value;

        Guid? parentId = dir != null
            ? dir.ParentDirectoryId
            : (parentDirectoryId ?? VaultsState.Value.SelectedDirectoryId);

        string? parentName = null;
        if (parentId.HasValue)
        {
            parentName = VaultsState.Value.Directories.FirstOrDefault(d => d.Id == parentId.Value)?.Name;
        }

        var parameters = new Dictionary<string, object?>
        {
            { "Name", dir?.Name ?? string.Empty },
            { "IsEdit", dir != null },
            { "ParentDirectoryName", parentName }
        };

        var result = await ModalService.ShowAsync<EditDirectoryModal>(parameters);
        if (result.IsSuccess && result.Data != null)
        {
            var newName = (string)result.Data;
            if (dir == null)
            {
                Dispatcher.Dispatch(new CreateDirectoryAction(vaultId, parentId, newName));
            }
            else
            {
                Dispatcher.Dispatch(new UpdateDirectoryAction(vaultId, dir.Id, dir.ParentDirectoryId, newName));
            }
        }
    }

    private async Task DeleteDirectory(DecryptedDirectory dir)
    {
        if (!VaultsState.Value.ActiveVaultId.HasValue) return;
        var vaultId = VaultsState.Value.ActiveVaultId.Value;

        var confirm = await ModalService.ShowConfirmationAsync($"Are you sure you want to delete directory '{dir.Name}'? All nested items will be lost.");
        if (confirm)
        {
            Dispatcher.Dispatch(new DeleteDirectoryAction(vaultId, dir.Id));
        }
    }

    private void HandleDirectoryMove((Guid SourceId, Guid? TargetId) moveAction)
    {
        if (!VaultsState.Value.ActiveVaultId.HasValue) return;
        var vaultId = VaultsState.Value.ActiveVaultId.Value;

        var dir = VaultsState.Value.Directories.FirstOrDefault(d => d.Id == moveAction.SourceId);
        if (dir != null)
        {
            Dispatcher.Dispatch(new UpdateDirectoryAction(vaultId, dir.Id, moveAction.TargetId, dir.Name));
        }
    }

    private void HandleRootDragOver(DragEventArgs e) { }

    private void HandleRootDrop(DragEventArgs e)
    {
        if (DragDropState.DraggedDirectoryId.HasValue)
        {
            HandleDirectoryMove((DragDropState.DraggedDirectoryId.Value, null));
        }
        DragDropState.DraggedDirectoryId = null;
    }
}
