using PassKee.Web.Constants.Navigation;
using PassKee.Web.Pages.App.Shared;
using PassKee.Web.Store.Ui;

namespace PassKee.Web.Pages.App.Mobile;

public partial class MobileVaultViewBlock : VaultOperationsComponentBase
{
    private bool _isSelectionMode;

    private void ToggleSelectionMode()
    {
        _isSelectionMode = !_isSelectionMode;
    }

    private void OnSelectionModeChanged(bool value)
    {
        _isSelectionMode = value;
    }

    private void HandleTabSelected(MobileNavigationTab tab)
    {
        _isSelectionMode = false;

        if (UiState.Value.ActiveMobileTab == tab && tab == MobileNavigationTab.Groups)
        {
            // Tapping Groups tab again while inside a subfolder returns to the root folder list
            SelectDirectory(null);
            return;
        }

        Dispatcher.Dispatch(new SetMobileNavigationTabAction(tab));
    }
}

