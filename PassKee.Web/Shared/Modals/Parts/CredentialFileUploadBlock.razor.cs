using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fluxor;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using PassKee.Api.Shared.Models.Storage;
using PassKee.Business.Common.Constants;
using PassKee.Web.Store.Vaults;

namespace PassKee.Web.Shared.Modals.Parts;

/// <summary>
/// Uploads a selected file right away through <see cref="UploadVaultFileAction"/> and reports the resulting
/// <see cref="StoredFileDto"/>. The file is attached to a credential only when that credential is saved.
/// </summary>
public partial class CredentialFileUploadBlock : ComponentBase
{
    [Inject] private IDispatcher Dispatcher { get; set; } = null!;

    [Parameter] public StoredFileDto? File { get; set; }
    [Parameter] public EventCallback<StoredFileDto?> FileChanged { get; set; }

    private const long MaxFileSize = FileStorageConstants.MaxFileSize;

    private bool _isUploading;
    private string? _uploadingFileName;
    private string? _errorMessage;

    private async Task UploadAsync(IReadOnlyList<IBrowserFile> files)
    {
        var browserFile = files.FirstOrDefault();
        if (browserFile == null || _isUploading) return;

        _errorMessage = null;
        _isUploading = true;
        _uploadingFileName = browserFile.Name;

        var requestId = Guid.NewGuid();
        var completion = new TaskCompletionSource<FileUploadResult>(TaskCreationOptions.RunContinuationsAsynchronously);
        try
        {
            Dispatcher.Dispatch(new UploadVaultFileAction(requestId, browserFile, completion));
            var result = await completion.Task;
            if (result.IsSuccess && result.RequestId == requestId)
            {
                File = result.File;
                await FileChanged.InvokeAsync(File);
            }
            else
            {
                _errorMessage = result.ErrorMessage ?? "Error uploading file";
            }
        }
        catch
        {
            _errorMessage = "Error uploading file";
        }
        finally
        {
            _isUploading = false;
            _uploadingFileName = null;
        }
    }

    private async Task RemoveAsync()
    {
        File = null;
        _errorMessage = null;
        await FileChanged.InvokeAsync(null);
    }

    private static string FormatSize(long bytes)
        => bytes >= 1024 * 1024 ? $"{bytes / 1024d / 1024d:0.#} MB" : $"{Math.Max(1, bytes / 1024)} KB";
}
