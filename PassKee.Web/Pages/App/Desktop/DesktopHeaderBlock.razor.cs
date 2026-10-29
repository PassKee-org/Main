using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using PassKee.Web.Pages.App.Shared;
using PassKee.Web.Store.Vaults;

namespace PassKee.Web.Pages.App.Desktop;

public partial class DesktopHeaderBlock : VaultOperationsComponentBase
{
    private string _localSearchQuery = string.Empty;
    private CancellationTokenSource? _debounceCts;

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
            HandleClearSearch();
        }
        else if (e.Key == "Enter")
        {
            _debounceCts?.Cancel();
            Dispatcher.Dispatch(new SetSearchQueryAction(_localSearchQuery));
        }
    }

    private void HandleClearSearch()
    {
        _debounceCts?.Cancel();
        _localSearchQuery = string.Empty;
        ClearSearch();
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

