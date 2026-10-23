using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fluxor;
using PassKee.Web.Core.Services.UI.Toast;
using PassKee.Web.Core.Utils;
using PassKee.Web.Models.Vaults;
using PassKee.Web.Services.Vaults;

namespace PassKee.Web.Store.Vaults.Effects;

public class DeleteCredentialEffect : Effect<DeleteCredentialAction>
{
    private readonly IVaultClientService _vaultService;
    private readonly IToastService _toastService;
    private readonly IState<VaultsState> _vaultsState;

    public DeleteCredentialEffect(
        IVaultClientService vaultService,
        IToastService toastService,
        IState<VaultsState> vaultsState)
    {
        _vaultService = vaultService;
        _toastService = toastService;
        _vaultsState = vaultsState;
    }

    public override async Task HandleAsync(DeleteCredentialAction action, IDispatcher dispatcher)
    {
        try
        {
            var targetCred = _vaultsState.Value.Credentials.FirstOrDefault(c => c.Id == action.CredentialId)
                             ?? _vaultsState.Value.ArchivedCredentials.FirstOrDefault(c => c.Id == action.CredentialId);

            List<Guid> orphanFileIds = [];
            if (targetCred != null)
            {
                orphanFileIds = await ResolveOrphanFilesAsync([targetCred], action.VaultId);
            }

            var res = await _vaultService.DeleteCredentialAsync(action.CredentialId);
            if (res != null)
            {
                foreach (var fileId in orphanFileIds)
                {
                    await _vaultService.DeleteFileAsync(fileId);
                }

                _toastService.ShowSuccess("Credential deleted");
                dispatcher.Dispatch(new DeleteCredentialSuccessAction(action.VaultId, action.CredentialId));
            }
        }
        catch
        {
            _toastService.ShowError("Error deleting credential");
        }
    }

    private async Task<List<Guid>> ResolveOrphanFilesAsync(
        List<DecryptedCredential> deletingCreds,
        Guid vaultId)
    {
        var hasAttachments = deletingCreds.Any(c =>
            CredentialAttachmentUtils.CollectAttachmentFileIds(c.Payload).Count > 0);

        if (!hasAttachments)
        {
            return [];
        }

        var allKnown = new List<DecryptedCredential>(_vaultsState.Value.Credentials);
        var archived = _vaultsState.Value.ArchivedCredentials;

        if (archived.Count == 0 && _vaultsState.Value.ActiveVaultKey != null)
        {
            try
            {
                var fetched = await _vaultService.GetArchivedCredentialsAsync(vaultId, _vaultsState.Value.ActiveVaultKey);
                allKnown.AddRange(fetched);
            }
            catch
            {
                allKnown.AddRange(archived);
            }
        }
        else
        {
            allKnown.AddRange(archived);
        }

        return CredentialAttachmentUtils.FindOrphanFileIds(deletingCreds, allKnown);
    }
}
