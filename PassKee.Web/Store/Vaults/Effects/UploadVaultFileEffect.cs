using System;
using System.IO;
using System.Threading.Tasks;
using Fluxor;
using PassKee.Business.Common.Constants;
using PassKee.Web.Services.Vaults;

namespace PassKee.Web.Store.Vaults.Effects;

public class UploadVaultFileEffect : Effect<UploadVaultFileAction>
{
    private readonly IVaultClientService _vaultService;
    private readonly IState<VaultsState> _vaultsState;

    public UploadVaultFileEffect(IVaultClientService vaultService, IState<VaultsState> vaultsState)
    {
        _vaultService = vaultService;
        _vaultsState = vaultsState;
    }

    public override async Task HandleAsync(UploadVaultFileAction action, IDispatcher dispatcher)
    {
        try
        {
            var vaultState = _vaultsState.Value;
            var vaultKey = vaultState.ActiveVaultKey;
            var vaultId = vaultState.ActiveVaultId;
            if (vaultKey == null || !vaultId.HasValue)
            {
                action.Completion.TrySetResult(new FileUploadResult(action.RequestId, null, "Vault is locked or not selected."));
                return;
            }

            using var memoryStream = new MemoryStream();
            await using (var stream = action.BrowserFile.OpenReadStream(FileStorageConstants.MaxFileSize))
            {
                await stream.CopyToAsync(memoryStream);
            }

            var file = await _vaultService.UploadFileAsync(vaultId.Value, memoryStream.ToArray(), action.BrowserFile.Name, vaultKey);
            if (file == null)
            {
                action.Completion.TrySetResult(new FileUploadResult(action.RequestId, null, "Error uploading file"));
                return;
            }

            file.FileName = action.BrowserFile.Name;
            action.Completion.TrySetResult(new FileUploadResult(action.RequestId, file, null));
        }
        catch
        {
            action.Completion.TrySetResult(new FileUploadResult(action.RequestId, null, "Error uploading file"));
        }
        finally
        {
            dispatcher.Dispatch(new UploadVaultFileFinishedAction(action.RequestId));
        }
    }
}
