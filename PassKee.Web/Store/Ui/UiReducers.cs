using Fluxor;

namespace PassKee.Web.Store.Ui;

public static class UiReducers
{
    [ReducerMethod]
    public static UiState ToggleMainMenuActionReducer(UiState state, ToggleMainMenuAction action)
        => state with { IsMainMenuOpened = !state.IsMainMenuOpened };

    [ReducerMethod]
    public static UiState SetMobileNavigationTabActionReducer(UiState state, SetMobileNavigationTabAction action)
        => state with { ActiveMobileTab = action.Tab };

    [ReducerMethod]
    public static UiState SetCredentialSortOrderActionReducer(UiState state, SetCredentialSortOrderAction action)
        => state with { SortOrder = action.SortOrder };

    [ReducerMethod]
    public static UiState SetMobileSearchOpenActionReducer(UiState state, SetMobileSearchOpenAction action)
        => state with { IsMobileSearchOpen = action.IsOpen };
}
