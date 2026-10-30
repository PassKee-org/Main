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
public class MoveCredentialFolderSelectBlockTests
{
    private readonly Guid _vaultId = Guid.NewGuid();
    private readonly Guid _dir1Id = Guid.NewGuid();
    private readonly Guid _dir2Id = Guid.NewGuid();
    private readonly Guid _subDir1Id = Guid.NewGuid();

    private List<DecryptedDirectory> CreateSampleDirectories()
    {
        return
        [
            new DecryptedDirectory(_dir1Id, _vaultId, null, "Folder A"),
            new DecryptedDirectory(_dir2Id, _vaultId, null, "Folder B"),
            new DecryptedDirectory(_subDir1Id, _vaultId, _dir1Id, "SubFolder A1")
        ];
    }

    [Fact]
    public void Folders_WhenRootSelected_ReturnsRootLevelDirectories()
    {
        var block = new MoveCredentialFolderSelectBlock
        {
            Directories = CreateSampleDirectories(),
            SelectedFolderId = null
        };

        var foldersProp = typeof(MoveCredentialFolderSelectBlock).GetProperty("Folders", BindingFlags.NonPublic | BindingFlags.Instance);
        var folders = foldersProp!.GetValue(block) as IReadOnlyList<DecryptedDirectory>;

        Assert.NotNull(folders);
        Assert.Equal(2, folders.Count);
        Assert.Contains(folders, f => f.Id == _dir1Id);
        Assert.Contains(folders, f => f.Id == _dir2Id);
    }

    [Fact]
    public void Folders_WhenSubFolderParentSelected_ReturnsOnlySubDirectories()
    {
        var block = new MoveCredentialFolderSelectBlock
        {
            Directories = CreateSampleDirectories(),
            SelectedFolderId = _dir1Id
        };

        var foldersProp = typeof(MoveCredentialFolderSelectBlock).GetProperty("Folders", BindingFlags.NonPublic | BindingFlags.Instance);
        var folders = foldersProp!.GetValue(block) as IReadOnlyList<DecryptedDirectory>;

        Assert.NotNull(folders);
        Assert.Single(folders);
        Assert.Equal(_subDir1Id, folders[0].Id);
    }

    [Fact]
    public void Path_WhenRootSelected_ReturnsOnlyRootCrumb()
    {
        var block = new MoveCredentialFolderSelectBlock
        {
            Directories = CreateSampleDirectories(),
            SelectedFolderId = null
        };

        var pathProp = typeof(MoveCredentialFolderSelectBlock).GetProperty("Path", BindingFlags.NonPublic | BindingFlags.Instance);
        var path = pathProp!.GetValue(block) as IReadOnlyList<(Guid? Id, string Name)>;

        Assert.NotNull(path);
        Assert.Single(path);
        Assert.Null(path[0].Id);
        Assert.Equal("Root (No folder)", path[0].Name);
    }

    [Fact]
    public void Path_WhenDeepFolderSelected_ReturnsFullBreadcrumb()
    {
        var block = new MoveCredentialFolderSelectBlock
        {
            Directories = CreateSampleDirectories(),
            SelectedFolderId = _subDir1Id
        };

        var pathProp = typeof(MoveCredentialFolderSelectBlock).GetProperty("Path", BindingFlags.NonPublic | BindingFlags.Instance);
        var path = pathProp!.GetValue(block) as IReadOnlyList<(Guid? Id, string Name)>;

        Assert.NotNull(path);
        Assert.Equal(3, path.Count);
        Assert.Null(path[0].Id);
        Assert.Equal("Root (No folder)", path[0].Name);
        Assert.Equal(_dir1Id, path[1].Id);
        Assert.Equal("Folder A", path[1].Name);
        Assert.Equal(_subDir1Id, path[2].Id);
        Assert.Equal("SubFolder A1", path[2].Name);
    }

    [Fact]
    public async Task GoBack_WhenSubFolderSelected_NavigatesToParent()
    {
        Guid? changedFolderId = null;
        var block = new MoveCredentialFolderSelectBlock
        {
            Directories = CreateSampleDirectories(),
            SelectedFolderId = _subDir1Id,
            SelectedFolderIdChanged = EventCallback.Factory.Create<Guid?>(this, id => changedFolderId = id)
        };

        var goBackMethod = typeof(MoveCredentialFolderSelectBlock).GetMethod("GoBack", BindingFlags.NonPublic | BindingFlags.Instance);
        var task = goBackMethod!.Invoke(block, null) as Task;
        await task!;

        Assert.Equal(_dir1Id, changedFolderId);
    }
}

