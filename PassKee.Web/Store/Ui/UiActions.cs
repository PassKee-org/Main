using PassKee.Web.Constants;
using PassKee.Web.Constants.Navigation;

namespace PassKee.Web.Store.Ui;

public record struct ToggleMainMenuAction();
public record SetMobileNavigationTabAction(MobileNavigationTab Tab);
public record SetCredentialSortOrderAction(CredentialSortOrder SortOrder);
public record SetMobileSearchOpenAction(bool IsOpen);
