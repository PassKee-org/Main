using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using PassKee.Web.Models.Vaults;

namespace PassKee.Web.Shared.Modals.Parts;

public partial class MoveCredentialFolderSelectBlock : ComponentBase
{
    [Parameter]
    public IReadOnlyList<DecryptedDirectory> Directories { get; set; } = Array.Empty<DecryptedDirectory>();

    [Parameter]
    public Guid? SelectedFolderId { get; set; }

    [Parameter]
    public EventCallback<Guid?> SelectedFolderIdChanged { get; set; }

    private IReadOnlyList<DecryptedDirectory> Folders => Directories
        .Where(item => item.ParentDirectoryId == SelectedFolderId)
        .OrderBy(item => item.Name, StringComparer.CurrentCultureIgnoreCase)
        .ToList();

    private IReadOnlyList<(Guid? Id, string Name)> Path
    {
        get
        {
            var foldersById = Directories.ToDictionary(item => item.Id);
            var path = new List<(Guid? Id, string Name)>();
            var visitedFolderIds = new HashSet<Guid>();
            var folderId = SelectedFolderId;

            while (folderId.HasValue && visitedFolderIds.Add(folderId.Value) && foldersById.TryGetValue(folderId.Value, out var folder))
            {
                path.Add((folder.Id, folder.Name));
                folderId = folder.ParentDirectoryId;
            }

            path.Reverse();
            path.Insert(0, (null, "Root (No folder)"));
            return path;
        }
    }

    private Task OpenFolder(Guid folderId)
    {
        return SelectedFolderIdChanged.InvokeAsync(folderId);
    }

    private Task SelectFolder(Guid? folderId)
    {
        return SelectedFolderIdChanged.InvokeAsync(folderId);
    }

    private Task GoBack()
    {
        var parentId = Directories
            .FirstOrDefault(item => item.Id == SelectedFolderId)?.ParentDirectoryId;
        return SelectedFolderIdChanged.InvokeAsync(parentId);
    }
}

