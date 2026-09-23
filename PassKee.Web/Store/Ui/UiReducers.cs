using Fluxor;

namespace PassKee.Web.Store.Ui;

public static class UiReducers
{
    [ReducerMethod]
    public static UiState ToggleMainMenuActionReducer(UiState state, ToggleMainMenuAction action)
    {
        return state with
        {
            IsMainMenuOpened = !state.IsMainMenuOpened
        };
    }
}

