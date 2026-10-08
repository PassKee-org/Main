using System;
using System.Collections.Generic;
using Fluxor;
using System.Linq;
using PassKee.Web.Models.Vaults;

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
    public static VaultsState ReduceUploadVaultFileAction(VaultsState state, UploadVaultFileAction action)
        => state with { PendingFileUploads = state.PendingFileUploads + 1 };

    [ReducerMethod]
    public static VaultsState ReduceUploadVaultFileFinishedAction(VaultsState state, UploadVaultFileFinishedAction action)
        => state with { PendingFileUploads = Math.Max(0, state.PendingFileUploads - 1) };

    [ReducerMethod]
    public static VaultsState ReduceSelectVaultAction(VaultsState state, SelectVaultAction action)
        => state with { ActiveVaultId = action.VaultId, SelectedDirectoryId = null, SearchQuery = string.Empty };

    [ReducerMethod]
    public static VaultsState ReduceSelectDirectoryAction(VaultsState state, SelectDirectoryAction action)
        => state with { SelectedDirectoryId = action.DirectoryId };

    [ReducerMethod]
    public static VaultsState ReduceDeleteDirectoryAction(VaultsState state, DeleteDirectoryAction action)
    {
        if (state.SelectedDirectoryId == action.DirectoryId || IsDescendantDirectory(state.Directories, action.DirectoryId, state.SelectedDirectoryId))
        {
            return state with { SelectedDirectoryId = null };
        }
        return state;
    }

    [ReducerMethod]
    public static VaultsState ReduceCreateDirectorySuccessAction(VaultsState state, CreateDirectorySuccessAction action)
        => state with
        {
            Directories = state.Directories.Any(d => d.Id == action.Directory.Id)
                ? state.Directories
                : state.Directories.Append(action.Directory).ToList()
        };

    [ReducerMethod]
    public static VaultsState ReduceUpdateDirectorySuccessAction(VaultsState state, UpdateDirectorySuccessAction action)
        => state with
        {
            Directories = state.Directories.Any(d => d.Id == action.Directory.Id)
                ? state.Directories.Select(d => d.Id == action.Directory.Id ? action.Directory : d).ToList()
                : state.Directories.Append(action.Directory).ToList()
        };

    [ReducerMethod]
    public static VaultsState ReduceDeleteDirectorySuccessAction(VaultsState state, DeleteDirectorySuccessAction action)
    {
        var deletedDirIds = new HashSet<Guid>(action.DeletedDirectoryIds ?? []);
        deletedDirIds.Add(action.DirectoryId);
        CollectAllDescendantIds(state.Directories, action.DirectoryId, deletedDirIds);

        var deletedCredIds = new HashSet<Guid>(action.DeletedCredentialIds ?? []);

        var remainingDirectories = state.Directories
            .Where(d => !deletedDirIds.Contains(d.Id))
            .ToList();

        var remainingCredentials = state.Credentials
            .Where(c => !deletedCredIds.Contains(c.Id) && !(c.DirectoryId.HasValue && deletedDirIds.Contains(c.DirectoryId.Value)))
            .ToList();

        var selectedDirectoryId = state.SelectedDirectoryId.HasValue && deletedDirIds.Contains(state.SelectedDirectoryId.Value)
            ? null
            : state.SelectedDirectoryId;

        return state with
        {
            Directories = remainingDirectories,
            Credentials = remainingCredentials,
            SelectedDirectoryId = selectedDirectoryId
        };
    }

    private static void CollectAllDescendantIds(List<DecryptedDirectory> directories, Guid parentId, HashSet<Guid> result)
    {
        foreach (var dir in directories)
        {
            if (dir.ParentDirectoryId == parentId && result.Add(dir.Id))
            {
                CollectAllDescendantIds(directories, dir.Id, result);
            }
        }
    }

    [ReducerMethod]
    public static VaultsState ReduceCreateCredentialSuccessAction(VaultsState state, CreateCredentialSuccessAction action)
        => state with
        {
            Credentials = state.Credentials.Any(c => c.Id == action.Credential.Id)
                ? state.Credentials
                : state.Credentials.Append(action.Credential).ToList()
        };

    [ReducerMethod]
    public static VaultsState ReduceUpdateCredentialSuccessAction(VaultsState state, UpdateCredentialSuccessAction action)
        => state with
        {
            Credentials = state.Credentials.Any(c => c.Id == action.Credential.Id)
                ? state.Credentials.Select(c => c.Id == action.Credential.Id ? action.Credential : c).ToList()
                : state.Credentials.Append(action.Credential).ToList()
        };

    [ReducerMethod]
    public static VaultsState ReduceDeleteCredentialSuccessAction(VaultsState state, DeleteCredentialSuccessAction action)
        => state with
        {
            Credentials = state.Credentials.Where(c => c.Id != action.CredentialId).ToList()
        };

    private static bool IsDescendantDirectory(List<DecryptedDirectory> directories, Guid parentId, Guid? targetId)
    {
        if (!targetId.HasValue) return false;
        var current = targetId;
        var visited = new HashSet<Guid>();
        while (current.HasValue && visited.Add(current.Value))
        {
            var dir = directories.FirstOrDefault(d => d.Id == current.Value);
            if (dir == null) break;
            if (dir.ParentDirectoryId == parentId) return true;
            current = dir.ParentDirectoryId;
        }
        return false;
    }

    [ReducerMethod]
    public static VaultsState ReduceLoadVaultDetailsAction(VaultsState state, LoadVaultDetailsAction action)
        => state with { IsLoading = true };

    [ReducerMethod]
    public static VaultsState ReduceLoadVaultDetailsSuccessAction(VaultsState state, LoadVaultDetailsSuccessAction action)
        => state.ActiveVaultId != action.VaultId ? state : state with {
            IsLoading = false, 
            ActiveVaultId = action.VaultId,
            ActiveVaultKey = action.ActiveVaultKey,
            Directories = action.Directories, 
            Credentials = action.Credentials,
            Tags = action.Tags,
            SelectedDirectoryId = state.SelectedDirectoryId.HasValue && action.Directories.Any(d => d.Id == state.SelectedDirectoryId.Value)
                ? state.SelectedDirectoryId
                : null
        };

    [ReducerMethod]
    public static VaultsState ReduceCreateTagSuccessAction(VaultsState state, CreateTagSuccessAction action)
        => state with
        {
            Tags = state.Tags.Any(t => t.Id == action.Tag.Id)
                ? state.Tags
                : state.Tags.Append(action.Tag).ToList()
        };

    [ReducerMethod]
    public static VaultsState ReduceUpdateTagSuccessAction(VaultsState state, UpdateTagSuccessAction action)
        => state with
        {
            Tags = state.Tags.Select(t => t.Id == action.Tag.Id ? action.Tag : t).ToList()
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
            ActiveVaultId = action.Vault.Id,
            SelectedDirectoryId = null
        };

    [ReducerMethod]
    public static VaultsState ReduceCreateVaultFailureAction(VaultsState state, CreateVaultFailureAction action)
        => state with { IsCreating = false };

    [ReducerMethod]
    public static VaultsState ReduceSetSearchQueryAction(VaultsState state, SetSearchQueryAction action)
        => state with { SearchQuery = action.Query };

    [ReducerMethod]
    public static VaultsState ReduceClearSearchQueryAction(VaultsState state, ClearSearchQueryAction action)
        => state with { SearchQuery = string.Empty };

    [ReducerMethod]
    public static VaultsState ReduceResetVaultsStateAction(VaultsState state, ResetVaultsStateAction action)
        => new();
}
