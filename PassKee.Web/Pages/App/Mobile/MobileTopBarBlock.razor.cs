using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using PassKee.Web.Constants;
using PassKee.Web.Constants.Navigation;
using PassKee.Web.Pages.App.Shared;
using PassKee.Web.Store.Ui;
using PassKee.Web.Store.Vaults;

namespace PassKee.Web.Pages.App.Mobile;

public partial class MobileTopBarBlock : VaultOperationsComponentBase
{
    [Parameter] public bool IsSelectionMode { get; set; }
    [Parameter] public EventCallback OnToggleSelectionMode { get; set; }

    private string _localSearchQuery = string.Empty;
    private CancellationTokenSource? _debounceCts;

    private bool IsSearchVisible => UiState.Value.IsMobileSearchOpen || IsSearchActive;

    private bool CanNavigateBack =>
        IsSearchActive ||
        (UiState.Value.ActiveMobileTab == MobileNavigationTab.Groups &&
         (VaultsState.Value.SelectedDirectoryId.HasValue || VaultsState.Value.IsArchiveSelected));

    private string HeaderTitle => UiState.Value.ActiveMobileTab switch
    {
        MobileNavigationTab.Home => "Home",
        MobileNavigationTab.Entries => "All Entries",
        _ => VaultsState.Value.IsArchiveSelected
            ? "Archive"
            : SelectedDirectory?.Name ?? (string.IsNullOrWhiteSpace(ActiveVaultName) ? "Directories" : ActiveVaultName)
    };

    private string HeaderIcon => UiState.Value.ActiveMobileTab switch
    {
        MobileNavigationTab.Home => "fa-solid fa-house text-sky-600",
        MobileNavigationTab.Entries => "fa-solid fa-list-ul text-sky-600",
        _ => VaultsState.Value.IsArchiveSelected
            ? "fa-solid fa-box-archive text-amber-500"
            : SelectedDirectory != null
                ? "fa-solid fa-folder text-sky-600"
                : "fa-solid fa-layer-group text-sky-600"
    };

    protected override void OnInitialized()
    {
        base.OnInitialized();
        _localSearchQuery = VaultsState.Value.SearchQuery;
    }

    protected override void OnParametersSet()
    {
        base.OnParametersSet();
        if (string.IsNullOrEmpty(VaultsState.Value.SearchQuery) && !string.IsNullOrEmpty(_localSearchQuery))
        {
            _localSearchQuery = string.Empty;
        }
    }

    private void HandleBackClick()
    {
        if (IsSearchActive)
        {
            CloseAndClearSearch();
            return;
        }

        NavigateUpDirectory();
    }

    private void ToggleSearchBar()
    {
        if (IsSearchVisible)
        {
            CloseAndClearSearch();
        }
        else
        {
            Dispatcher.Dispatch(new SetMobileSearchOpenAction(true));
        }
    }

    private void CloseAndClearSearch()
    {
        _debounceCts?.Cancel();
        _localSearchQuery = string.Empty;
        ClearSearch();
        Dispatcher.Dispatch(new SetMobileSearchOpenAction(false));
    }

    private void SetSortOrder(CredentialSortOrder sortOrder)
    {
        Dispatcher.Dispatch(new SetCredentialSortOrderAction(sortOrder));
    }

    private void NavigateToArchive()
    {
        SelectArchive();
        if (UiState.Value.ActiveMobileTab != MobileNavigationTab.Groups)
        {
            Dispatcher.Dispatch(new SetMobileNavigationTabAction(MobileNavigationTab.Groups));
        }
    }

    private void HandleSearchInput(ChangeEventArgs e)
    {
        _localSearchQuery = e.Value?.ToString() ?? string.Empty;
        _debounceCts?.Cancel();
        _debounceCts?.Dispose();
        _debounceCts = new CancellationTokenSource();
        var token = _debounceCts.Token;

        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(200, token);
                await InvokeAsync(() => Dispatcher.Dispatch(new SetSearchQueryAction(_localSearchQuery)));
            }
            catch (OperationCanceledException)
            {
            }
        });
    }

    private void HandleSearchKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Escape")
        {
            CloseAndClearSearch();
        }
        else if (e.Key == "Enter")
        {
            _debounceCts?.Cancel();
            Dispatcher.Dispatch(new SetSearchQueryAction(_localSearchQuery));
        }
    }

    protected override ValueTask DisposeAsyncCore(bool disposing)
    {
        if (disposing)
        {
            _debounceCts?.Cancel();
            _debounceCts?.Dispose();
        }
        return base.DisposeAsyncCore(disposing);
    }
}

