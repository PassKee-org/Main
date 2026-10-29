using Microsoft.AspNetCore.Components.Web;
using PassKee.Web.Pages.App.Desktop.Parts;
using PassKee.Web.Pages.App.Shared;

namespace PassKee.Web.Pages.App.Desktop;

public partial class DesktopSidebarBlock : VaultOperationsComponentBase
{
    private void HandleRootDragOver(DragEventArgs e)
    {
    }

    private void HandleRootDrop(DragEventArgs e)
    {
        if (DesktopDragDropState.DraggedDirectoryId.HasValue)
        {
            HandleDirectoryMove((DesktopDragDropState.DraggedDirectoryId.Value, null));
        }
        DesktopDragDropState.DraggedDirectoryId = null;
    }
}

