using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using PassKee.Web.Models.Vaults;
using PassKee.Web.Shared.Modals.Parts;
using Xunit;

namespace PassKee.Tests.Unit.Web.Components;

[SuppressMessage("Usage", "BL0005", Justification = "Unit tests for component logic.")]
public class MoveFolderSelectBlockTests
{
    private readonly Guid _vaultId = Guid.NewGuid();
    private readonly Guid _dirA = Guid.NewGuid();
    private readonly Guid _dirB = Guid.NewGuid();
    private readonly Guid _subDirA1 = Guid.NewGuid();
    private readonly Guid _subDirA11 = Guid.NewGuid();

    private List<DecryptedDirectory> CreateSampleDirectories()
    {
        return
        [
            new DecryptedDirectory(_dirA, _vaultId, null, "Folder A"),
            new DecryptedDirectory(_dirB, _vaultId, null, "Folder B"),
            new DecryptedDirectory(_subDirA1, _vaultId, _dirA, "SubFolder A1"),
            new DecryptedDirectory(_subDirA11, _vaultId, _subDirA1, "SubFolder A1-1")
        ];
    }

    private static T? GetProtectedProperty<T>(object target, string propertyName)
    {
        var prop = target.GetType().GetProperty(propertyName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
        return (T?)prop?.GetValue(target);
    }

    [Fact]
    public void Folders_WhenRootSelected_ReturnsRootLevelDirectories()
    {
        var block = new MoveFolderSelectBlock
        {
            Directories = CreateSampleDirectories(),
            SelectedFolderId = null
        };

        var folders = GetProtectedProperty<IReadOnlyList<DecryptedDirectory>>(block, "Folders");

        Assert.NotNull(folders);
        Assert.Equal(2, folders.Count);
        Assert.Contains(folders, f => f.Id == _dirA);
        Assert.Contains(folders, f => f.Id == _dirB);
    }

    [Fact]
    public void Folders_WhenExcludedDirectoryIdIsRootFolder_ExcludesItFromRootList()
    {
        var block = new MoveFolderSelectBlock
        {
            Directories = CreateSampleDirectories(),
            SelectedFolderId = null,
            ExcludedDirectoryId = _dirA
        };

        var folders = GetProtectedProperty<IReadOnlyList<DecryptedDirectory>>(block, "Folders");

        Assert.NotNull(folders);
        Assert.Single(folders);
        Assert.Equal(_dirB, folders[0].Id);
    }

    [Fact]
    public void Folders_WhenExcludedDirectoryHasDescendants_ExcludesDescendantsRecursively()
    {
        var block = new MoveFolderSelectBlock
        {
            Directories = CreateSampleDirectories(),
            SelectedFolderId = _dirA,
            ExcludedDirectoryId = _dirA
        };

        var excludedIds = GetProtectedProperty<HashSet<Guid>>(block, "ExcludedIds");

        Assert.NotNull(excludedIds);
        Assert.Contains(_dirA, excludedIds);
        Assert.Contains(_subDirA1, excludedIds);
        Assert.Contains(_subDirA11, excludedIds);
        Assert.DoesNotContain(_dirB, excludedIds);

        var folders = GetProtectedProperty<IReadOnlyList<DecryptedDirectory>>(block, "Folders");
        Assert.NotNull(folders);
        Assert.Empty(folders);
    }

    [Fact]
    public void DestinationSummary_ReflectsRootAndCurrentFolderIndicator()
    {
        var blockRoot = new MoveFolderSelectBlock
        {
            Directories = CreateSampleDirectories(),
            SelectedFolderId = null,
            CurrentFolderId = null
        };

        var destName = GetProtectedProperty<string>(blockRoot, "CurrentDestinationName");
        var isCurrent = GetProtectedProperty<bool>(blockRoot, "IsCurrentFolder");

        Assert.Equal("Root (No folder)", destName);
        Assert.True(isCurrent);

        var blockOther = new MoveFolderSelectBlock
        {
            Directories = CreateSampleDirectories(),
            SelectedFolderId = _dirA,
            CurrentFolderId = _dirB
        };

        var destNameOther = GetProtectedProperty<string>(blockOther, "CurrentDestinationName");
        var isCurrentOther = GetProtectedProperty<bool>(blockOther, "IsCurrentFolder");

        Assert.Equal("Folder A", destNameOther);
        Assert.False(isCurrentOther);
    }

    [Fact]
    public void Path_WhenDeepFolderSelected_ReturnsFullBreadcrumb()
    {
        var block = new MoveFolderSelectBlock
        {
            Directories = CreateSampleDirectories(),
            SelectedFolderId = _subDirA11
        };

        var path = GetProtectedProperty<IReadOnlyList<(Guid? Id, string Name)>>(block, "Path");

        Assert.NotNull(path);
        Assert.Equal(4, path.Count);
        Assert.Null(path[0].Id);
        Assert.Equal("Root (No folder)", path[0].Name);
        Assert.Equal(_dirA, path[1].Id);
        Assert.Equal("Folder A", path[1].Name);
        Assert.Equal(_subDirA1, path[2].Id);
        Assert.Equal("SubFolder A1", path[2].Name);
        Assert.Equal(_subDirA11, path[3].Id);
        Assert.Equal("SubFolder A1-1", path[3].Name);
    }

    [Fact]
    public async Task GoBack_NavigatesToParent()
    {
        Guid? changedFolderId = null;
        var block = new MoveFolderSelectBlock
        {
            Directories = CreateSampleDirectories(),
            SelectedFolderId = _subDirA1,
            SelectedFolderIdChanged = EventCallback.Factory.Create<Guid?>(this, id => changedFolderId = id)
        };

        var goBackMethod = typeof(MoveFolderSelectBlock).GetMethod("GoBack", BindingFlags.NonPublic | BindingFlags.Instance);
        var task = goBackMethod!.Invoke(block, null) as Task;
        await task!;

        Assert.Equal(_dirA, changedFolderId);
    }
}

