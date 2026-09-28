using System;
using System.Threading.Tasks;
using Fluxor;
using PassKee.Web.Core.Services.UI.Toast;
using PassKee.Web.Services.Vaults;

namespace PassKee.Web.Store.Vaults.Effects;

public class DeleteDirectoryEffect : Effect<DeleteDirectoryAction>
{
    private readonly IVaultClientService _vaultService;
    private readonly IToastService _toastService;

    public DeleteDirectoryEffect(IVaultClientService vaultService, IToastService toastService)
    {
        _vaultService = vaultService;
        _toastService = toastService;
    }

    public override async Task HandleAsync(DeleteDirectoryAction action, IDispatcher dispatcher)
    {
        try
        {
            var ok = await _vaultService.DeleteDirectoryAsync(action.DirectoryId);
            if (ok)
            {
                _toastService.ShowSuccess("Directory deleted");
                dispatcher.Dispatch(new LoadVaultDetailsAction(action.VaultId));
            }
        }
        catch
        {
            _toastService.ShowError("Error deleting directory");
        }
    }
}
