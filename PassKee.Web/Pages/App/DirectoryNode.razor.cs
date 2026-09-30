using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using PassKee.Web.Models.Vaults;

namespace PassKee.Web.Pages.App;

public partial class DirectoryNode : ComponentBase
{
    [Parameter] public DecryptedDirectory Directory { get; set; } = null!;
    [Parameter] public IEnumerable<DecryptedDirectory> AllDirectories { get; set; } = new List<DecryptedDirectory>();
    [Parameter] public Guid? SelectedDirectoryId { get; set; }
    
    [Parameter] public EventCallback<Guid?> OnSelect { get; set; }
    [Parameter] public EventCallback<DecryptedDirectory> OnAddSubdirectory { get; set; }
    [Parameter] public EventCallback<(Guid SourceId, Guid? TargetId)> OnMove { get; set; }
    [Parameter] public EventCallback<DecryptedDirectory> OnEdit { get; set; }
    [Parameter] public EventCallback<DecryptedDirectory> OnDelete { get; set; }

    private bool IsExpanded { get; set; }
    private bool IsDragged { get; set; }
    private bool IsDragTarget { get; set; }
    private Guid? _previousSelectedDirectoryId;

    private IEnumerable<DecryptedDirectory> Children => AllDirectories.Where(d => d.ParentDirectoryId == Directory.Id);
    private bool HasChildren => Children.Any();

    protected override void OnInitialized()
    {
        CheckAndExpandIfDescendantSelected();
    }

    protected override void OnParametersSet()
    {
        if (SelectedDirectoryId != _previousSelectedDirectoryId)
        {
            _previousSelectedDirectoryId = SelectedDirectoryId;
            CheckAndExpandIfDescendantSelected();
        }
    }

    private void CheckAndExpandIfDescendantSelected()
    {
        if (SelectedDirectoryId == Directory.Id || IsDescendantSelected(Directory.Id))
        {
            IsExpanded = true;
        }
    }

    private bool IsDescendantSelected(Guid parentId)
    {
        if (!SelectedDirectoryId.HasValue) return false;
        foreach (var child in AllDirectories.Where(d => d.ParentDirectoryId == parentId))
        {
            if (child.Id == SelectedDirectoryId.Value || IsDescendantSelected(child.Id))
            {
                return true;
            }
        }
        return false;
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
        var current = AllDirectories.FirstOrDefault(d => d.Id == potentialDescendantId);
        while (current?.ParentDirectoryId != null)
        {
            if (current.ParentDirectoryId == ancestorId) return true;
            current = AllDirectories.FirstOrDefault(d => d.Id == current.ParentDirectoryId);
        }
        return false;
    }
}

public static class DragDropState
{
    public static Guid? DraggedDirectoryId { get; set; }
}
