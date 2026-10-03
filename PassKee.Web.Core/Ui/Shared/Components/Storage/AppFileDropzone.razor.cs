using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace PassKee.Web.Core.Ui.Shared.Components.Storage;

public partial class AppFileDropzone : ComponentBase
{
    [Parameter] public string Class { get; set; } = string.Empty;
    [Parameter] public string? Title { get; set; }
    [Parameter] public string? Subtitle { get; set; }
    [Parameter] public string Accept { get; set; } = "*/*";
    [Parameter] public bool Multiple { get; set; }
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public long MaxFileSize { get; set; } = 20 * 1024 * 1024; // 20MB by default

    [Parameter] public IBrowserFile? File { get; set; }
    [Parameter] public EventCallback<IBrowserFile?> FileChanged { get; set; }
    
    [Parameter] public EventCallback<IReadOnlyList<IBrowserFile>> OnFilesSelected { get; set; }

    private string? _errorMessage;

    private async Task OnInputFileChange(InputFileChangeEventArgs e)
    {
        _errorMessage = null;
        var files = e.GetMultipleFiles(Multiple ? 10 : 1);
        var validFiles = new List<IBrowserFile>();

        foreach (var file in files)
        {
            if (file.Size > MaxFileSize)
            {
                _errorMessage = $"File {file.Name} is larger than {MaxFileSize / 1024 / 1024}MB max size limit.";
                continue;
            }
            validFiles.Add(file);
        }

        if (validFiles.Any())
        {
            if (!Multiple)
            {
                File = validFiles.First();
                await FileChanged.InvokeAsync(File);
            }
            await OnFilesSelected.InvokeAsync(validFiles);
        }
    }

    private async Task OnRemoveFile()
    {
        File = null;
        _errorMessage = null;
        await FileChanged.InvokeAsync(null);
    }
}
