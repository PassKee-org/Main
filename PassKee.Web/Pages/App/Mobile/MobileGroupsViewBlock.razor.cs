using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using PassKee.Web.Models.Vaults;
using PassKee.Web.Pages.App.Shared;
using PassKee.Web.Store.Vaults;
using PassKee.Web.Utils;

namespace PassKee.Web.Pages.App.Mobile;

public partial class MobileGroupsViewBlock : VaultOperationsComponentBase
{
    [Parameter] public bool IsSelectionMode { get; set; }
    [Parameter] public EventCallback<bool> IsSelectionModeChanged { get; set; }

    private readonly HashSet<Guid> _selectedCredentialIds = [];

    private IReadOnlyList<DecryptedDirectory> FilteredDirectories
    {
        get
        {
            if (!IsSearchActive || VaultsState.Value.IsArchiveSelected)
            {
                return [];
            }

            var snapshot = Hierarchy;
            return VaultSearchService.FilterDirectories(
                VaultsState.Value.Directories,
                SearchQuery.Trim(),
                snapshot.DirectoryPaths);
        }
    }

    private IReadOnlyList<DecryptedCredential> DisplayedCredentials
    {
        get
        {
            var snapshot = Hierarchy;
            var isArchive = VaultsState.Value.IsArchiveSelected;
            var source = isArchive ? VaultsState.Value.ArchivedCredentials : VaultsState.Value.Credentials;

            IReadOnlyList<DecryptedCredential> baseList;
            if (IsSearchActive)
            {
                baseList = VaultSearchService.Filter(source, SearchQuery.Trim(), snapshot.TagNames);
            }
            else if (isArchive)
            {
                baseList = source;
            }
            else
            {
                baseList = snapshot.GetDirectoryCredentials(VaultsState.Value.SelectedDirectoryId);
            }

            return VaultHierarchySnapshot.ApplySort(baseList, UiState.Value.SortOrder);
        }
    }

    private void HandleSelectDirectory(Guid? directoryId)
    {
        if (IsSearchActive)
        {
            ClearSearch();
        }
        SelectDirectory(directoryId);
    }

    private async Task EnterSelectionModeWithCredential(DecryptedCredential cred)
    {
        _selectedCredentialIds.Clear();
        _selectedCredentialIds.Add(cred.Id);
        await IsSelectionModeChanged.InvokeAsync(true);
    }

    private async Task ExitSelectionMode()
    {
        _selectedCredentialIds.Clear();
        await IsSelectionModeChanged.InvokeAsync(false);
    }

    private void ToggleCredentialSelection(Guid id, bool selected)
    {
        if (selected)
        {
            _selectedCredentialIds.Add(id);
        }
        else
        {
            _selectedCredentialIds.Remove(id);
        }
    }

    private void ToggleSelectAll()
    {
        var current = DisplayedCredentials;
        if (_selectedCredentialIds.Count >= current.Count)
        {
            _selectedCredentialIds.Clear();
        }
        else
        {
            foreach (var cred in current)
            {
                _selectedCredentialIds.Add(cred.Id);
            }
        }
    }

    private async Task ArchiveSelectedCredentialsAsync()
    {
        if (!VaultsState.Value.ActiveVaultId.HasValue || _selectedCredentialIds.Count == 0)
        {
            return;
        }

        var vaultId = VaultsState.Value.ActiveVaultId.Value;
        var count = _selectedCredentialIds.Count;
        var confirm = await ModalService.ShowConfirmationAsync(
            $"Are you sure you want to archive {count} {(count == 1 ? "entry" : "entries")}?");
        if (!confirm)
        {
            return;
        }

        var ids = _selectedCredentialIds.ToList();
        await ExitSelectionMode();
        foreach (var id in ids)
        {
            Dispatcher.Dispatch(new ArchiveCredentialAction(vaultId, id));
        }
    }

    private async Task DeleteSelectedCredentialsAsync()
    {
        if (!VaultsState.Value.ActiveVaultId.HasValue || _selectedCredentialIds.Count == 0)
        {
            return;
        }

        var vaultId = VaultsState.Value.ActiveVaultId.Value;
        var count = _selectedCredentialIds.Count;
        var confirm = await ModalService.ShowConfirmationAsync(
            $"Are you sure you want to permanently delete {count} {(count == 1 ? "entry" : "entries")}? This action cannot be undone.");
        if (!confirm)
        {
            return;
        }

        var ids = _selectedCredentialIds.ToList();
        await ExitSelectionMode();
        Dispatcher.Dispatch(new DeleteCredentialsAction(vaultId, ids));
    }
}

