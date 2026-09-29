using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using PassKee.Api.Shared.Models.Vaults;
using PassKee.Web.Models.Vaults;

namespace PassKee.Web.Pages.App;

public partial class VaultSidebarBlock : ComponentBase
{
    [Parameter]
    public Guid? ActiveVaultId { get; set; }

    [Parameter]
    public IReadOnlyList<VaultDto> Vaults { get; set; } = [];

    [Parameter]
    public IReadOnlyList<DecryptedDirectory> Directories { get; set; } = [];

    [Parameter]
    public Guid? SelectedDirectoryId { get; set; }

    [Parameter]
    public bool IsCreatingVault { get; set; }

    [Parameter]
    public bool IsVaultsLoading { get; set; }

    [Parameter]
    public EventCallback<Guid?> OnSelectVault { get; set; }

    [Parameter]
    public EventCallback OnCreateVault { get; set; }

    [Parameter]
    public EventCallback<Guid?> OnSelectDirectory { get; set; }

    [Parameter]
    public EventCallback<DecryptedDirectory?> OnOpenDirectoryModal { get; set; }

    [Parameter]
    public EventCallback<DecryptedDirectory> OnDeleteDirectory { get; set; }

    [Parameter]
    public EventCallback<(Guid SourceId, Guid? TargetId)> OnMoveDirectory { get; set; }

    private IEnumerable<Guid?> VaultIds => Vaults.Select(vault => (Guid?)vault.Id);

    private string GetVaultName(Guid? vaultId) =>
        Vaults.FirstOrDefault(vault => vault.Id == vaultId)?.Name ?? string.Empty;

    private void HandleRootDragOver(DragEventArgs e) { }

    private void HandleRootDrop(DragEventArgs e)
    {
        if (DragDropState.DraggedDirectoryId.HasValue)
        {
            OnMoveDirectory.InvokeAsync((DragDropState.DraggedDirectoryId.Value, null));
        }
        DragDropState.DraggedDirectoryId = null;
    }
}
