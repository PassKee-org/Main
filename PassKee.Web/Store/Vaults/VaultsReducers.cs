using Fluxor;
using System.Linq;

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
    public static VaultsState ReduceCreateVaultAction(VaultsState state, CreateVaultAction action)
        => state with { IsCreating = true };

    [ReducerMethod]
    public static VaultsState ReduceCreateVaultSuccessAction(VaultsState state, CreateVaultSuccessAction action)
        => state with
        {
            IsCreating = false,
            Vaults = state.Vaults.Any(vault => vault.Id == action.Vault.Id)
                ? state.Vaults
                : state.Vaults.Append(action.Vault).ToList(),
            ActiveVaultId = action.Vault.Id
        };

    [ReducerMethod]
    public static VaultsState ReduceCreateVaultFailureAction(VaultsState state, CreateVaultFailureAction action)
        => state with { IsCreating = false };

    [ReducerMethod]
    public static VaultsState ReduceResetVaultsStateAction(VaultsState state, ResetVaultsStateAction action)
        => new();
}
