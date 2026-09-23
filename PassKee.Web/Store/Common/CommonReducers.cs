using Fluxor;

namespace PassKee.Web.Store.Common;

public static class CommonReducers
{
    [ReducerMethod]
    public static CommonState SetIsAppInitializedActionReducer(CommonState state, SetIsAppInitializedAction action)
    {
        return state with
        {
            IsAppInitialized = action.IsInitialized
        };
    }
}

