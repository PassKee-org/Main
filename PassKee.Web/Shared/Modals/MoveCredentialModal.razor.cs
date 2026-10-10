using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fluxor;
using Microsoft.AspNetCore.Components;
using PassKee.Web.Core.Services.UI.Modal;
using PassKee.Web.Core.Services.UI.Toast;
using PassKee.Web.Models.Vaults;
using PassKee.Web.Store.Vaults;

namespace PassKee.Web.Shared.Modals;

public partial class MoveCredentialModal : ComponentBase
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
    public DecryptedCredential Credential { get; set; } = null!;

    private Guid? _selectedFolderId;
    private Guid? _credentialId;
    private bool _isSaving;

    private IReadOnlyList<DecryptedDirectory> Directories => VaultsState.Value.Directories;

    private string CurrentDestinationName => _selectedFolderId.HasValue
        ? Directories.FirstOrDefault(d => d.Id == _selectedFolderId)?.Name ?? "Directory"
        : "Root (No folder)";

    protected override void OnParametersSet()
    {
        if (Credential == null)
        {
            _credentialId = null;
            return;
        }

        if (_credentialId != Credential.Id)
        {
            _credentialId = Credential.Id;
            _selectedFolderId = Credential.DirectoryId;
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
        if (Credential == null || _isSaving) return;
        if (_selectedFolderId == Credential.DirectoryId) return;

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
            var completion = new TaskCompletionSource<CredentialSaveResult>(TaskCreationOptions.RunContinuationsAsynchronously);

            Dispatcher.Dispatch(new UpdateCredentialAction(
                requestId,
                Credential.Id,
                _selectedFolderId,
                Credential.Type,
                Credential.Payload,
                completion));

            var result = await completion.Task;
            if (result.IsSuccess)
            {
                ToastService.ShowSuccess("Item moved successfully");
                if (ModalInstance != null)
                {
                    ModalService.Close(ModalInstance, AppModalResult.Ok(_selectedFolderId));
                }
            }
            else
            {
                ToastService.ShowError(result.ErrorMessage ?? "Error moving item");
            }
        }
        catch
        {
            ToastService.ShowError("Error moving item");
        }
        finally
        {
            _isSaving = false;
        }
    }
}
