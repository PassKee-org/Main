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

public class DeleteCredentialsEffect : Effect<DeleteCredentialsAction>
{
    private readonly IVaultClientService _vaultService;
    private readonly IToastService _toastService;
    private readonly IState<VaultsState> _vaultsState;

    public DeleteCredentialsEffect(
        IVaultClientService vaultService,
        IToastService toastService,
        IState<VaultsState> vaultsState)
    {
        _vaultService = vaultService;
        _toastService = toastService;
        _vaultsState = vaultsState;
    }

    public override async Task HandleAsync(DeleteCredentialsAction action, IDispatcher dispatcher)
    {
        try
        {
            if (action.CredentialIds == null || action.CredentialIds.Count == 0)
            {
                return;
            }

            var idSet = action.CredentialIds.ToHashSet();
            var targetCreds = _vaultsState.Value.Credentials
                .Concat(_vaultsState.Value.ArchivedCredentials)
                .Where(c => idSet.Contains(c.Id))
                .ToList();

            var orphanFileIds = await ResolveOrphanFilesAsync(targetCreds, action.VaultId);

            var res = await _vaultService.DeleteCredentialsAsync(action.CredentialIds);
            if (res != null)
            {
                foreach (var fileId in orphanFileIds)
                {
                    await _vaultService.DeleteFileAsync(fileId);
                }

                var count = res.CredentialIds.Count;
                var message = count == 1 ? "1 credential deleted" : $"{count} credentials deleted";
                _toastService.ShowSuccess(message);
                dispatcher.Dispatch(new DeleteCredentialsSuccessAction(action.VaultId, res.CredentialIds));
            }
        }
        catch
        {
            _toastService.ShowError("Error deleting credentials");
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

