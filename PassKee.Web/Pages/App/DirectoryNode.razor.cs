using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using PassKee.Web.Models.Vaults;

namespace PassKee.Web.Pages.App;

public partial class DirectoryNode : ComponentBase
{
    [Parameter] public DecryptedDirectory Directory { get; set; } = null!;
    [Parameter] public IReadOnlyDictionary<Guid, List<DecryptedDirectory>> ChildrenMap { get; set; } = new Dictionary<Guid, List<DecryptedDirectory>>();
    [Parameter] public IReadOnlyDictionary<Guid, DecryptedDirectory> DirectoryMap { get; set; } = new Dictionary<Guid, DecryptedDirectory>();
    [Parameter] public Guid? SelectedDirectoryId { get; set; }
    [Parameter] public IReadOnlySet<Guid>? ExpandedAncestorIds { get; set; }

    [Parameter] public EventCallback<Guid?> OnSelect { get; set; }
    [Parameter] public EventCallback<DecryptedDirectory> OnAddSubdirectory { get; set; }
    [Parameter] public EventCallback<(Guid SourceId, Guid? TargetId)> OnMove { get; set; }
    [Parameter] public EventCallback<DecryptedDirectory> OnEdit { get; set; }
    [Parameter] public EventCallback<DecryptedDirectory> OnDelete { get; set; }

    private bool IsExpanded { get; set; }
    private bool IsDragged { get; set; }
    private bool IsDragTarget { get; set; }

    private IReadOnlyList<DecryptedDirectory> Children =>
        ChildrenMap.TryGetValue(Directory.Id, out var list) ? list : Array.Empty<DecryptedDirectory>();

    private bool HasChildren => Children.Count > 0;

    protected override void OnInitialized()
    {
        CheckAndExpandIfDescendantSelected();
    }

    protected override void OnParametersSet()
    {
        CheckAndExpandIfDescendantSelected();
    }

    private void CheckAndExpandIfDescendantSelected()
    {
        if (ExpandedAncestorIds != null && ExpandedAncestorIds.Contains(Directory.Id))
        {
            IsExpanded = true;
        }
    }

    private async Task HandleSelect()
    {
        if (HasChildren && !IsExpanded)
        {
            IsExpanded = true;
        }
        await OnSelect.InvokeAsync(Directory.Id);
    }

    private void ToggleExpand()
    {
        IsExpanded = !IsExpanded;
    }

    private void HandleDragStart(DragEventArgs e)
    {
        IsDragged = true;
        DragDropState.DraggedDirectoryId = Directory.Id;
    }

    private void HandleDragEnd(DragEventArgs e)
    {
        IsDragged = false;
        DragDropState.DraggedDirectoryId = null;
    }

    private void HandleDragOver(DragEventArgs e)
    {
        if (DragDropState.DraggedDirectoryId.HasValue 
            && DragDropState.DraggedDirectoryId.Value != Directory.Id
            && !IsDescendantOf(Directory.Id, DragDropState.DraggedDirectoryId.Value))
        {
            IsDragTarget = true;
        }
    }

    private void HandleDragLeave(DragEventArgs e)
    {
        IsDragTarget = false;
    }

    private async Task HandleDrop(DragEventArgs e)
    {
        IsDragTarget = false;
        if (DragDropState.DraggedDirectoryId.HasValue 
            && DragDropState.DraggedDirectoryId.Value != Directory.Id
            && !IsDescendantOf(Directory.Id, DragDropState.DraggedDirectoryId.Value))
        {
            await OnMove.InvokeAsync((DragDropState.DraggedDirectoryId.Value, Directory.Id));
        }
        DragDropState.DraggedDirectoryId = null;
    }

    private bool IsDescendantOf(Guid potentialDescendantId, Guid ancestorId)
    {
        var currentId = (Guid?)potentialDescendantId;
        var visited = new HashSet<Guid>();
        while (currentId.HasValue && visited.Add(currentId.Value) && DirectoryMap.TryGetValue(currentId.Value, out var current))
        {
            if (current.ParentDirectoryId == ancestorId) return true;
            currentId = current.ParentDirectoryId;
        }
        return false;
    }
}

public static class DragDropState
{
    public static Guid? DraggedDirectoryId { get; set; }
}
