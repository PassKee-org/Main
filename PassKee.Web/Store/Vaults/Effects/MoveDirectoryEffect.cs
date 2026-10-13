using System;
using System.Linq;
using System.Threading.Tasks;
using Fluxor;
using PassKee.Web.Core.Services.UI.Toast;
using PassKee.Web.Models.Vaults;
using PassKee.Web.Services.Vaults;

namespace PassKee.Web.Store.Vaults.Effects;

public class MoveDirectoryEffect : Effect<MoveDirectoryAction>
{
    private readonly IVaultClientService _vaultService;
    private readonly IToastService _toastService;
    private readonly IState<VaultsState> _vaultsState;

    public MoveDirectoryEffect(
        IVaultClientService vaultService,
        IToastService toastService,
        IState<VaultsState> vaultsState)
    {
        _vaultService = vaultService;
        _toastService = toastService;
        _vaultsState = vaultsState;
    }

    public override async Task HandleAsync(MoveDirectoryAction action, IDispatcher dispatcher)
    {
        try
        {
            var vaultState = _vaultsState.Value;
            var vaultId = vaultState.ActiveVaultId;
            if (!vaultId.HasValue)
            {
                var errorMsg = "Vault is locked or not selected.";
                _toastService.ShowError(errorMsg);
                action.Completion?.TrySetResult(new DirectorySaveResult(action.RequestId, vaultId, null, null, errorMsg));
                return;
            }

            var dir = vaultState.Directories.FirstOrDefault(d => d.Id == action.DirectoryId);
            if (dir == null)
            {
                var errorMsg = "Directory not found.";
                _toastService.ShowError(errorMsg);
                action.Completion?.TrySetResult(new DirectorySaveResult(action.RequestId, vaultId, null, null, errorMsg));
                return;
            }

            var updated = await _vaultService.MoveDirectoryAsync(action.DirectoryId, action.DestinationFolderId);
            if (updated == null)
            {
                var errorMsg = "Error moving directory";
                _toastService.ShowError(errorMsg);
                action.Completion?.TrySetResult(new DirectorySaveResult(action.RequestId, vaultId, null, null, errorMsg));
                return;
            }

            var decryptedDir = new DecryptedDirectory(updated.Id, updated.VaultId, updated.ParentDirectoryId, dir.Name);
            dispatcher.Dispatch(new UpdateDirectorySuccessAction(decryptedDir));

            if (action.Completion == null)
            {
                _toastService.ShowSuccess("Directory moved");
            }
            else
            {
                action.Completion.TrySetResult(new DirectorySaveResult(action.RequestId, vaultId, updated.Id, updated.ParentDirectoryId, null));
            }
        }
        catch (Exception ex)
        {
            _toastService.ShowError("Error moving directory");
            action.Completion?.TrySetResult(new DirectorySaveResult(action.RequestId, null, null, null, ex.Message));
        }
    }
}

