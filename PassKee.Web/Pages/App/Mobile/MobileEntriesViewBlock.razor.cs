using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using PassKee.Business.Common.Constants;
using PassKee.Web.Models.Vaults;
using PassKee.Web.Pages.App.Shared;
using PassKee.Web.Store.Vaults;
using PassKee.Web.Utils;

namespace PassKee.Web.Pages.App.Mobile;

public partial class MobileEntriesViewBlock : VaultOperationsComponentBase
{
    [Parameter] public bool IsSelectionMode { get; set; }
    [Parameter] public EventCallback<bool> IsSelectionModeChanged { get; set; }

    private CredentialType? _selectedType;
    private readonly HashSet<Guid> _selectedCredentialIds = [];

    private IEnumerable<CredentialType> AvailableTypes =>
        VaultsState.Value.Credentials
            .Select(c => c.Type)
            .Distinct()
            .OrderBy(t => (int)t);

    private IReadOnlyList<DecryptedCredential> DisplayedEntries
    {
        get
        {
            var snapshot = Hierarchy;
            IEnumerable<DecryptedCredential> list = VaultsState.Value.Credentials;

            if (IsSearchActive)
            {
                list = VaultSearchService.Filter(list, SearchQuery.Trim(), snapshot.TagNames);
            }

            if (_selectedType.HasValue)
            {
                var filterType = _selectedType.Value;
                list = list.Where(c => c.Type == filterType);
            }

            return VaultHierarchySnapshot.ApplySort(list, UiState.Value.SortOrder);
        }
    }

    private void SelectTypeFilter(CredentialType? type)
    {
        _selectedType = _selectedType == type ? null : type;
    }

    private void ClearFilters()
    {
        _selectedType = null;
        if (IsSearchActive)
        {
            ClearSearch();
        }
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
        var current = DisplayedEntries;
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

    private async Task ArchiveSelectedEntriesAsync()
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
}

