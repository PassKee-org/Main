using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fluxor;
using Microsoft.AspNetCore.Components;
using PassKee.Web.Core.Services.UI.Modal;
using PassKee.Web.Core.Services.UI.Toast;
using PassKee.Web.Models.Vaults;
using PassKee.Web.Store.Vaults;

namespace PassKee.Web.Shared.Modals;

public partial class MoveDirectoryModal : ComponentBase
{
    [CascadingParameter]
    public AppModalInstance? ModalInstance { get; set; }

    [Inject]
    private IDispatcher Dispatcher { get; set; } = null!;

    [Inject]
    private IToastService ToastService { get; set; } = null!;

    [Inject]
    private IState<VaultsState> VaultsState { get; set; } = null!;

    [Inject]
    private IAppModalDialogService ModalService { get; set; } = null!;

    [Parameter]
    public DecryptedDirectory Directory { get; set; } = null!;

    private Guid? _selectedFolderId;
    private Guid? _directoryId;
    private bool _isSaving;

    private IReadOnlyList<DecryptedDirectory> Directories => VaultsState.Value.Directories;

    protected override void OnParametersSet()
    {
        if (Directory == null)
        {
            _directoryId = null;
            return;
        }

        if (_directoryId != Directory.Id)
        {
            _directoryId = Directory.Id;
            _selectedFolderId = Directory.ParentDirectoryId;
        }
    }

    private void OnFolderChanged(Guid? folderId)
    {
        _selectedFolderId = folderId;
    }

    private void Cancel()
    {
        if (_isSaving) return;
        if (ModalInstance != null)
        {
            ModalService.Close(ModalInstance, AppModalResult.Cancel());
        }
    }

    private async Task SubmitAsync()
    {
        if (Directory == null || _isSaving) return;
        if (_selectedFolderId == Directory.ParentDirectoryId) return;

        var activeVaultId = VaultsState.Value.ActiveVaultId;
        if (!activeVaultId.HasValue)
        {
            ToastService.ShowError("Vault is locked or not selected.");
            return;
        }

        _isSaving = true;
        try
        {
            var requestId = Guid.NewGuid();
            var completion = new TaskCompletionSource<DirectorySaveResult>(TaskCreationOptions.RunContinuationsAsynchronously);

            Dispatcher.Dispatch(new MoveDirectoryAction(
                requestId,
                activeVaultId.Value,
                Directory.Id,
                _selectedFolderId,
                completion));

            var result = await completion.Task;
            if (result.IsSuccess)
            {
                ToastService.ShowSuccess("Directory moved successfully");
                if (ModalInstance != null)
                {
                    ModalService.Close(ModalInstance, AppModalResult.Ok(_selectedFolderId));
                }
            }
            else
            {
                ToastService.ShowError(result.ErrorMessage ?? "Error moving directory");
            }
        }
        catch
        {
            ToastService.ShowError("Error moving directory");
        }
        finally
        {
            _isSaving = false;
        }
    }
}

