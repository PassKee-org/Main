using System.Collections.Generic;
using System.Linq;
using PassKee.Web.Constants.Navigation;
using PassKee.Web.Models.Vaults;
using PassKee.Web.Pages.App.Shared;
using PassKee.Web.Store.Ui;

namespace PassKee.Web.Pages.App.Mobile;

public partial class MobileHomeViewBlock : VaultOperationsComponentBase
{
    private IReadOnlyList<DecryptedCredential> QuickAccessCredentials =>
        VaultsState.Value.Credentials.Take(5).ToList();

    private void NavigateToRootGroups()
    {
        SelectDirectory(null);
        Dispatcher.Dispatch(new SetMobileNavigationTabAction(MobileNavigationTab.Groups));
    }

    private void NavigateToAllEntries()
    {
        Dispatcher.Dispatch(new SetMobileNavigationTabAction(MobileNavigationTab.Entries));
    }

    private void NavigateToArchive()
    {
        SelectArchive();
        Dispatcher.Dispatch(new SetMobileNavigationTabAction(MobileNavigationTab.Groups));
    }
}

