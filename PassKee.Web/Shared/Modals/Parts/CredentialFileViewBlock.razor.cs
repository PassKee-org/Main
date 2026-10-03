using System;
using System.Threading.Tasks;
using Fluxor;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using PassKee.Api.Shared.Models.Storage;
using PassKee.Web.Core.Services.UI.Toast;
using PassKee.Web.Services.Vaults;
using PassKee.Web.Store.Vaults;

namespace PassKee.Web.Shared.Modals.Parts;

public partial class CredentialFileViewBlock : ComponentBase
{
    [Inject] private IVaultClientService VaultClient { get; set; } = null!;
    [Inject] private IToastService ToastService { get; set; } = null!;
    [Inject] private IJSRuntime JSRuntime { get; set; } = null!;
    [Inject] private IState<VaultsState> VaultsState { get; set; } = null!;

    [Parameter] public string Label { get; set; } = string.Empty;
    [Parameter] public StoredFileDto? File { get; set; }

    private bool IsDownloading { get; set; }

    private async Task DownloadAsync()
    {
        if (File == null) return;

        IsDownloading = true;
        try
        {
            var vaultKey = VaultsState.Value.ActiveVaultKey;
            if (vaultKey == null)
            {
                ToastService.ShowError("Vault key not found.");
                return;
            }

            var bytes = await VaultClient.DownloadFileAsync(File.Id, vaultKey);
            if (bytes == null)
            {
                ToastService.ShowError("Failed to download or decrypt file.");
                return;
            }

            using var streamRef = new DotNetStreamReference(stream: new System.IO.MemoryStream(bytes));
            await JSRuntime.InvokeVoidAsync("PassKeeInterop.downloadFileFromStream", File.FileName ?? "file", streamRef);
        }
        catch (Exception ex)
        {
            ToastService.ShowError($"Error: {ex.Message}");
        }
        finally
        {
            IsDownloading = false;
        }
    }
}
