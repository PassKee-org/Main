using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Components;
using PassKee.Api.Shared.Models.Vaults;
using PassKee.Web.Models.Vaults;

namespace PassKee.Web.Pages.App;

public partial class VaultDrawerBlock : ComponentBase
{
    [Parameter]
    public bool IsOpen { get; set; }

    [Parameter]
    public EventCallback OnClose { get; set; }

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
}
