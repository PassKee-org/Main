using PassKee.Web.Constants;
using PassKee.Web.Constants.Navigation;
using PassKee.Web.Store.Ui;
using Xunit;

namespace PassKee.Tests.Unit.Web.Store;

public class UiReducersTests
{
    [Fact]
    public void SetMobileNavigationTabActionReducer_UpdatesActiveMobileTab()
    {
        var initial = new UiState();
        Assert.Equal(MobileNavigationTab.Groups, initial.ActiveMobileTab);

        var updatedHome = UiReducers.SetMobileNavigationTabActionReducer(initial, new SetMobileNavigationTabAction(MobileNavigationTab.Home));
        Assert.Equal(MobileNavigationTab.Home, updatedHome.ActiveMobileTab);

        var updatedEntries = UiReducers.SetMobileNavigationTabActionReducer(updatedHome, new SetMobileNavigationTabAction(MobileNavigationTab.Entries));
        Assert.Equal(MobileNavigationTab.Entries, updatedEntries.ActiveMobileTab);
    }

    [Fact]
    public void SetCredentialSortOrderActionReducer_UpdatesSortOrder()
    {
        var initial = new UiState();
        Assert.Equal(CredentialSortOrder.Custom, initial.SortOrder);

        var updated = UiReducers.SetCredentialSortOrderActionReducer(initial, new SetCredentialSortOrderAction(CredentialSortOrder.TitleAsc));
        Assert.Equal(CredentialSortOrder.TitleAsc, updated.SortOrder);
    }

    [Fact]
    public void SetMobileSearchOpenActionReducer_UpdatesIsMobileSearchOpen()
    {
        var initial = new UiState();
        Assert.False(initial.IsMobileSearchOpen);

        var opened = UiReducers.SetMobileSearchOpenActionReducer(initial, new SetMobileSearchOpenAction(true));
        Assert.True(opened.IsMobileSearchOpen);

        var closed = UiReducers.SetMobileSearchOpenActionReducer(opened, new SetMobileSearchOpenAction(false));
        Assert.False(closed.IsMobileSearchOpen);
    }
}

