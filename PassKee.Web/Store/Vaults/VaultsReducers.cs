using Fluxor;

namespace PassKee.Web.Store.Vaults;

public static class VaultsReducers
{
    [ReducerMethod]
    public static VaultsState ReduceLoadVaultsAction(VaultsState state, LoadVaultsAction action)
        => state with { IsLoading = true };

    [ReducerMethod]
    public static VaultsState ReduceLoadVaultsSuccessAction(VaultsState state, LoadVaultsSuccessAction action)
        => state with { IsLoading = false, Vaults = action.Vaults };

    [ReducerMethod]
    public static VaultsState ReduceLoadVaultsFailureAction(VaultsState state, LoadVaultsFailureAction action)
        => state with { IsLoading = false };

    [ReducerMethod]
    public static VaultsState ReduceSelectVaultAction(VaultsState state, SelectVaultAction action)
        => state with { ActiveVaultId = action.VaultId };

    [ReducerMethod]
    public static VaultsState ReduceLoadVaultDetailsAction(VaultsState state, LoadVaultDetailsAction action)
        => state with { IsLoading = true };

    [ReducerMethod]
    public static VaultsState ReduceLoadVaultDetailsSuccessAction(VaultsState state, LoadVaultDetailsSuccessAction action)
        => state with { 
            IsLoading = false, 
            ActiveVaultId = action.VaultId,
            ActiveVaultKey = action.ActiveVaultKey,
            Directories = action.Directories, 
            Credentials = action.Credentials 
        };

    [ReducerMethod]
    public static VaultsState ReduceLoadVaultDetailsFailureAction(VaultsState state, LoadVaultDetailsFailureAction action)
        => state with { IsLoading = false };

    [ReducerMethod]
    public static VaultsState ReduceResetVaultsStateAction(VaultsState state, ResetVaultsStateAction action)
        => new();
}
