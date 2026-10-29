using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PassKee.Web.Models.Vaults;
using PassKee.Web.Pages.App.Shared;
using PassKee.Web.Store.Vaults;

namespace PassKee.Web.Pages.App.Desktop;

public partial class DesktopCredentialListBlock : VaultOperationsComponentBase
{
    private string? _cachedCredsQuery;
    private Guid? _cachedSelectedDirId;
    private bool _cachedIsArchiveSelected;
    private List<DecryptedCredential>? _cachedCredentialsRef;
    private ICollection<DecryptedCredential>? _cachedFilteredCredentials;

    private string? _cachedDirsQuery;
    private List<DecryptedDirectory>? _cachedDirsRef;
    private IReadOnlyList<DecryptedDirectory>? _cachedFilteredDirectories;

    private readonly HashSet<Guid> _selectedArchivedCredentialIds = [];
    private bool _isDeletingArchived;

    private bool HasArchivedSelection => VaultsState.Value.IsArchiveSelected && _selectedArchivedCredentialIds.Count > 0;
    private bool IsReorderEnabled => !IsSearchActive && !VaultsState.Value.IsArchiveSelected;

    private ICollection<DecryptedCredential> FilteredCredentials
    {
        get
        {
            var snapshot = Hierarchy;
            var query = SearchQuery?.Trim() ?? string.Empty;
            var isArchive = VaultsState.Value.IsArchiveSelected;
            var selectedDirId = isArchive ? null : VaultsState.Value.SelectedDirectoryId;
            var credentials = isArchive ? VaultsState.Value.ArchivedCredentials : VaultsState.Value.Credentials;

            if (ReferenceEquals(credentials, _cachedCredentialsRef) &&
                query == _cachedCredsQuery &&
                selectedDirId == _cachedSelectedDirId &&
                isArchive == _cachedIsArchiveSelected &&
                _cachedFilteredCredentials != null)
            {
                return _cachedFilteredCredentials;
            }

            _cachedCredentialsRef = credentials;
            _cachedCredsQuery = query;
            _cachedSelectedDirId = selectedDirId;
            _cachedIsArchiveSelected = isArchive;

            if (!isArchive && _selectedArchivedCredentialIds.Count > 0)
            {
                _selectedArchivedCredentialIds.Clear();
            }
            else if (isArchive && _selectedArchivedCredentialIds.Count > 0)
            {
                _selectedArchivedCredentialIds.RemoveWhere(id => credentials.All(c => c.Id != id));
            }

            if (!string.IsNullOrWhiteSpace(query))
            {
                _cachedFilteredCredentials = VaultSearchService.Filter(credentials, query, snapshot.TagNames);
            }
            else if (isArchive)
            {
                _cachedFilteredCredentials = credentials;
            }
            else
            {
                _cachedFilteredCredentials = snapshot.GetDirectoryCredentials(selectedDirId).ToList();
            }

            return _cachedFilteredCredentials;
        }
    }

    private IReadOnlyList<DecryptedDirectory> FilteredDirectories
    {
        get
        {
            if (!IsSearchActive || VaultsState.Value.IsArchiveSelected)
            {
                return [];
            }

            var snapshot = Hierarchy;
            var query = SearchQuery?.Trim() ?? string.Empty;
            var dirs = VaultsState.Value.Directories;

            if (ReferenceEquals(dirs, _cachedDirsRef) &&
                query == _cachedDirsQuery &&
                _cachedFilteredDirectories != null)
            {
                return _cachedFilteredDirectories;
            }

            _cachedDirsRef = dirs;
            _cachedDirsQuery = query;
            _cachedFilteredDirectories = VaultSearchService.FilterDirectories(dirs, query, snapshot.DirectoryPaths);
            return _cachedFilteredDirectories;
        }
    }

    private void HandleCredentialsReordered(IList<DecryptedCredential> reorderedCredentials)
    {
        if (!VaultsState.Value.ActiveVaultId.HasValue || reorderedCredentials.Count == 0)
        {
            return;
        }

        var positions = new Dictionary<Guid, int>(reorderedCredentials.Count);
        for (var i = 0; i < reorderedCredentials.Count; i++)
        {
            positions[reorderedCredentials[i].Id] = i;
        }

        Dispatcher.Dispatch(new UpdateCredentialPositionsAction(VaultsState.Value.ActiveVaultId.Value, positions));
    }

    private void ToggleSelectAllArchived(bool selectAll)
    {
        if (selectAll)
        {
            foreach (var cred in FilteredCredentials)
            {
                _selectedArchivedCredentialIds.Add(cred.Id);
            }
        }
        else
        {
            _selectedArchivedCredentialIds.Clear();
        }
    }

    private void ToggleArchivedCredential(Guid id, bool isSelected)
    {
        if (isSelected)
        {
            _selectedArchivedCredentialIds.Add(id);
        }
        else
        {
            _selectedArchivedCredentialIds.Remove(id);
        }
    }

    private void ClearArchivedSelection()
    {
        _selectedArchivedCredentialIds.Clear();
    }

    private async Task DeleteSingleArchivedItemAsync(DecryptedCredential cred)
    {
        if (!VaultsState.Value.ActiveVaultId.HasValue) return;
        var vaultId = VaultsState.Value.ActiveVaultId.Value;

        var confirm = await ModalService.ShowConfirmationAsync(
            $"Are you sure you want to permanently delete '{cred.Payload?.Title}'? This action cannot be undone.");
        if (confirm)
        {
            _selectedArchivedCredentialIds.Remove(cred.Id);
            Dispatcher.Dispatch(new DeleteCredentialAction(vaultId, cred.Id));
        }
    }

    private async Task DeleteSelectedArchivedCredentialsAsync()
    {
        if (!VaultsState.Value.ActiveVaultId.HasValue || _selectedArchivedCredentialIds.Count == 0 || _isDeletingArchived)
        {
            return;
        }

        var count = _selectedArchivedCredentialIds.Count;
        var entryWord = count == 1 ? "entry" : "entries";
        var confirm = await ModalService.ShowConfirmationAsync(
            $"Are you sure you want to permanently delete {count} archived {entryWord}? This action cannot be undone.");
        if (!confirm)
        {
            return;
        }

        _isDeletingArchived = true;
        try
        {
            var vaultId = VaultsState.Value.ActiveVaultId.Value;
            var idsToDelete = _selectedArchivedCredentialIds.ToList();
            _selectedArchivedCredentialIds.Clear();
            Dispatcher.Dispatch(new DeleteCredentialsAction(vaultId, idsToDelete));
        }
        finally
        {
            _isDeletingArchived = false;
        }
    }
}

