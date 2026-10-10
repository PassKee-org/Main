using System.Threading.Tasks;
using Fluxor;
using PassKee.Web.Core.Services.UI.Toast;
using PassKee.Web.Services.Vaults;

namespace PassKee.Web.Store.Vaults.Effects;

public class ArchiveCredentialEffect : Effect<ArchiveCredentialAction>
{
    private readonly IVaultClientService _vaultService;
    private readonly IToastService _toastService;

    public ArchiveCredentialEffect(IVaultClientService vaultService, IToastService toastService)
    {
        _vaultService = vaultService;
        _toastService = toastService;
    }

    public override async Task HandleAsync(ArchiveCredentialAction action, IDispatcher dispatcher)
    {
        try
        {
            var res = await _vaultService.ArchiveCredentialAsync(action.CredentialId);
            if (res != null)
            {
                _toastService.ShowSuccess("Credential archived");
                dispatcher.Dispatch(new ArchiveCredentialSuccessAction(action.VaultId, action.CredentialId));
            }
        }
        catch
        {
            _toastService.ShowError("Error archiving credential");
        }
    }
}

