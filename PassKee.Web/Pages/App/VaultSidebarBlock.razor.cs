using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fluxor;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using PassKee.Web.Components;
using PassKee.Web.Core.Services.UI.Modal;
using PassKee.Web.Models.Vaults;
using PassKee.Web.Shared.Modals;
using PassKee.Web.Store.Vaults;

namespace PassKee.Web.Pages.App;

public partial class VaultSidebarBlock : BaseReactiveComponent
{
    [Inject] public IState<VaultsState> VaultsState { get; set; } = null!;
    [Inject] public IState<PassKee.Web.Store.Import.ImportState> ImportState { get; set; } = null!;
    [Inject] public IState<PassKee.Web.Store.Export.ExportState> ExportState { get; set; } = null!;

    private bool CanImportOrExport =>
        VaultsState.Value.ActiveVaultId.HasValue &&
        VaultsState.Value.ActiveVaultKey != null &&
        !VaultsState.Value.IsLoading &&
        !ImportState.Value.IsRunning &&
        !ExportState.Value.IsRunning;

    private bool CanDeleteVault =>
        VaultsState.Value.ActiveVaultId.HasValue &&
        !VaultsState.Value.IsLoading &&
        !ImportState.Value.IsRunning &&
        !ExportState.Value.IsRunning;

    private async Task DeleteVault()
    {
        if (!CanDeleteVault) return;
        var vaultId = VaultsState.Value.ActiveVaultId!.Value;
        var vaultName = GetVaultName(vaultId);

        var confirm = await ModalService.ShowConfirmationAsync(
            $"Are you sure you want to delete vault '{vaultName}'? All entries and attachments will be deleted. This action cannot be undone.",
            title: "Delete Vault",
            confirmText: "Delete",
            cancelText: "Cancel",
            type: AppConfirmationType.Danger);

        if (confirm)
        {
            Dispatcher.Dispatch(new DeleteVaultAction(vaultId));
        }
    }

    private async Task OpenImportModal()
    {
        if (!CanImportOrExport) return;
        await ModalService.ShowAsync<KdbxImportModal>(options: new PassKee.Web.Core.Services.UI.Modal.AppModalOptions
        {
            Size = PassKee.Web.Core.Services.UI.Modal.AppModalSize.Medium,
            IsScrollable = true,
            HasCloseButton = false,
            IsCloseOnBackdropClick = false,
            IsCloseOnEscapeKey = false
        });
    }

    private async Task OpenExportModal()
    {
        if (!CanImportOrExport) return;
        await ModalService.ShowAsync<KdbxExportModal>(options: new PassKee.Web.Core.Services.UI.Modal.AppModalOptions
        {
            Size = PassKee.Web.Core.Services.UI.Modal.AppModalSize.Medium,
            IsScrollable = true,
            HasCloseButton = false,
            IsCloseOnBackdropClick = false,
            IsCloseOnEscapeKey = false
        });
    }

    [Parameter]
    public EventCallback OnItemNavigated { get; set; }

    private List<DecryptedDirectory>? _lastDirectoriesRef;
    private List<DecryptedCredential>? _lastCredentialsRef;
    private Guid? _lastSelectedDirectoryId;
    private Dictionary<Guid, DecryptedDirectory> _directoryMap = new();
    private Dictionary<Guid, List<DecryptedDirectory>> _childrenMap = new();
    private Dictionary<Guid, int> _directoryCounts = new();
    private int _rootCount;
    private HashSet<Guid> _expandedAncestorIds = new();
    private List<DecryptedDirectory> _rootDirectories = [];

    private void EnsureDirectoryStructure()
    {
        var dirs = VaultsState.Value.Directories;
        var selectedId = VaultsState.Value.SelectedDirectoryId;
        var credentials = VaultsState.Value.Credentials;

        if (!ReferenceEquals(_lastCredentialsRef, credentials))
        {
            _lastCredentialsRef = credentials;
            var counts = new Dictionary<Guid, int>();
            var rootCount = 0;
            foreach (var cred in credentials)
            {
                if (cred.DirectoryId.HasValue)
                {
                    counts[cred.DirectoryId.Value] = counts.GetValueOrDefault(cred.DirectoryId.Value) + 1;
                }
                else
                {
                    rootCount++;
                }
            }
            _directoryCounts = counts;
            _rootCount = rootCount;
        }

        if (!ReferenceEquals(_lastDirectoriesRef, dirs))
        {
            _lastDirectoriesRef = dirs;
            _directoryMap = dirs.ToDictionary(d => d.Id, d => d);

            var children = new Dictionary<Guid, List<DecryptedDirectory>>();
            var roots = new List<DecryptedDirectory>();
            foreach (var dir in dirs)
            {
                if (dir.ParentDirectoryId.HasValue)
                {
                    if (!children.TryGetValue(dir.ParentDirectoryId.Value, out var list))
                    {
                        list = [];
                        children[dir.ParentDirectoryId.Value] = list;
                    }
                    list.Add(dir);
                }
                else
                {
                    roots.Add(dir);
                }
            }
            _childrenMap = children;
            _rootDirectories = roots;
        }

        if (_lastSelectedDirectoryId != selectedId)
        {
            _lastSelectedDirectoryId = selectedId;
            var ancestors = new HashSet<Guid>();
            var currentId = selectedId;
            while (currentId.HasValue && _directoryMap.TryGetValue(currentId.Value, out var dir))
            {
                if (dir.ParentDirectoryId.HasValue)
                {
                    ancestors.Add(dir.ParentDirectoryId.Value);
                }
                currentId = dir.ParentDirectoryId;
            }
            _expandedAncestorIds = ancestors;
        }
    }

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

    private async Task SelectArchive()
    {
        var vaultId = VaultsState.Value.ActiveVaultId;
        if (vaultId.HasValue)
        {
            Dispatcher.Dispatch(new SelectArchiveAction());
            Dispatcher.Dispatch(new LoadArchivedCredentialsAction(vaultId.Value));
        }
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

        var confirm = await ModalService.ShowConfirmationAsync($"Are you sure you want to delete directory '{dir.Name}'? All nested items will be archived.");
        if (confirm)
        {
            Dispatcher.Dispatch(new DeleteDirectoryAction(vaultId, dir.Id));
        }
    }

    private async Task OpenMoveDirectoryModal(DecryptedDirectory dir)
    {
        var parameters = new Dictionary<string, object?>
        {
            { "Directory", dir }
        };

        await ModalService.ShowAsync<PassKee.Web.Shared.Modals.MoveDirectoryModal>(parameters, new AppModalOptions
        {
            Size = AppModalSize.Small
        });
    }

    private void HandleDirectoryMove((Guid SourceId, Guid? TargetId) moveAction)
    {
        if (!VaultsState.Value.ActiveVaultId.HasValue) return;
        var vaultId = VaultsState.Value.ActiveVaultId.Value;

        var dir = VaultsState.Value.Directories.FirstOrDefault(d => d.Id == moveAction.SourceId);
        if (dir != null)
        {
            Dispatcher.Dispatch(new MoveDirectoryAction(Guid.NewGuid(), vaultId, dir.Id, moveAction.TargetId));
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
