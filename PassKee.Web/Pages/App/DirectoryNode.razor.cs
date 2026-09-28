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
    
    [Parameter] public EventCallback OnSelect { get; set; }
    [Parameter] public EventCallback<(Guid SourceId, Guid? TargetId)> OnMove { get; set; }
    [Parameter] public EventCallback<DecryptedDirectory> OnEdit { get; set; }
    [Parameter] public EventCallback<DecryptedDirectory> OnDelete { get; set; }

    private bool IsExpanded { get; set; }
    private bool IsDragged { get; set; }
    private bool IsDragTarget { get; set; }

    private IEnumerable<DecryptedDirectory> Children => AllDirectories.Where(d => d.ParentDirectoryId == Directory.Id);
    private bool HasChildren => Children.Any();

    protected override void OnInitialized()
    {
        if (SelectedDirectoryId == Directory.Id)
        {
            IsExpanded = true;
        }
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
        if (DragDropState.DraggedDirectoryId.HasValue && DragDropState.DraggedDirectoryId != Directory.Id)
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
        if (DragDropState.DraggedDirectoryId.HasValue && DragDropState.DraggedDirectoryId != Directory.Id)
        {
            await OnMove.InvokeAsync((DragDropState.DraggedDirectoryId.Value, Directory.Id));
        }
        DragDropState.DraggedDirectoryId = null;
    }
}

public static class DragDropState
{
    public static Guid? DraggedDirectoryId { get; set; }
}
