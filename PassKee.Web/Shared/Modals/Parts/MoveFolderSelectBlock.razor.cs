using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using PassKee.Web.Models.Vaults;

namespace PassKee.Web.Shared.Modals.Parts;

public partial class MoveFolderSelectBlock : ComponentBase
{
    [Parameter]
    public IReadOnlyList<DecryptedDirectory> Directories { get; set; } = Array.Empty<DecryptedDirectory>();

    [Parameter]
    public Guid? SelectedFolderId { get; set; }

    [Parameter]
    public EventCallback<Guid?> SelectedFolderIdChanged { get; set; }

    [Parameter]
    public Guid? CurrentFolderId { get; set; }

    [Parameter]
    public Guid? ExcludedDirectoryId { get; set; }

    [Parameter]
    public bool ShowDestinationSummary { get; set; } = true;

    protected HashSet<Guid> ExcludedIds
    {
        get
        {
            var set = new HashSet<Guid>();
            if (!ExcludedDirectoryId.HasValue)
            {
                return set;
            }

            set.Add(ExcludedDirectoryId.Value);

            bool added;
            do
            {
                added = false;
                foreach (var dir in Directories)
                {
                    if (dir.ParentDirectoryId.HasValue && set.Contains(dir.ParentDirectoryId.Value) && set.Add(dir.Id))
                    {
                        added = true;
                    }
                }
            } while (added);

            return set;
        }
    }

    protected IReadOnlyList<DecryptedDirectory> Folders => Directories
        .Where(item => item.ParentDirectoryId == SelectedFolderId && !ExcludedIds.Contains(item.Id))
        .OrderBy(item => item.Name, StringComparer.CurrentCultureIgnoreCase)
        .ToList();

    protected IReadOnlyList<(Guid? Id, string Name)> Path
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

    protected string CurrentDestinationName => SelectedFolderId.HasValue
        ? Directories.FirstOrDefault(d => d.Id == SelectedFolderId)?.Name ?? "Directory"
        : "Root (No folder)";

    protected bool IsCurrentFolder => SelectedFolderId == CurrentFolderId;

    protected Task OpenFolder(Guid folderId)
    {
        return SelectedFolderIdChanged.InvokeAsync(folderId);
    }

    protected Task SelectFolder(Guid? folderId)
    {
        return SelectedFolderIdChanged.InvokeAsync(folderId);
    }

    protected Task GoBack()
    {
        var parentId = Directories
            .FirstOrDefault(item => item.Id == SelectedFolderId)?.ParentDirectoryId;
        return SelectedFolderIdChanged.InvokeAsync(parentId);
    }
}

