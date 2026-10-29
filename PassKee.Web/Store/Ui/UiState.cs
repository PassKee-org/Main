using Fluxor;
using PassKee.Web.Constants;
using PassKee.Web.Constants.Navigation;

namespace PassKee.Web.Store.Ui;

[FeatureState]
public record UiState
{
    public bool IsMainMenuOpened { get; init; } = true;
    public MobileNavigationTab ActiveMobileTab { get; init; } = MobileNavigationTab.Groups;
    public CredentialSortOrder SortOrder { get; init; } = CredentialSortOrder.Custom;
    public bool IsMobileSearchOpen { get; init; }
}
