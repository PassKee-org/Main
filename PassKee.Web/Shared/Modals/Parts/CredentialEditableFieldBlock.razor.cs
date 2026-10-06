using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using PassKee.Api.Shared.Models.Storage;
using PassKee.Api.Shared.Models.Vaults.Payloads;
using PassKee.Web.Core.Services.UI.Modal;

namespace PassKee.Web.Shared.Modals.Parts;

public partial class CredentialEditableFieldBlock : ComponentBase
{
    [Inject] private IAppModalDialogService ModalService { get; set; } = null!;

    [Parameter, EditorRequired] public CredentialField Field { get; set; } = null!;
    [Parameter] public EventCallback OnRemove { get; set; }
    [Parameter] public EventCallback OnFieldChanged { get; set; }

    private bool _isPasswordVisible;

    private string ResolvedFieldType => Field.Type switch
    {
        CredentialFieldType.Password => "password",
        CredentialFieldType.Date => "date",
        CredentialFieldType.Url => "url",
        CredentialFieldType.Email => "email",
        CredentialFieldType.Phone => "tel",
        _ => "text"
    };

    private string ResolvedFieldPlaceholder => Field.Type switch
    {
        CredentialFieldType.Address => "Address",
        CredentialFieldType.Password => "Password",
        CredentialFieldType.Date => "Date",
        CredentialFieldType.Url => "https://",
        CredentialFieldType.Email => "name@example.com",
        CredentialFieldType.Phone => "+1...",
        CredentialFieldType.SecurityQuestion => "Answer",
        CredentialFieldType.OneTimePassword => "Secret key / otpauth URI",
        _ => "Value"
    };

    private string ResolvedFieldInputClass => Field.Type switch
    {
        CredentialFieldType.Password => "text-sm font-normal font-mono text-gray-900 placeholder:text-gray-400",
        CredentialFieldType.Address => "text-sm font-normal text-gray-900 placeholder:text-gray-400 resize-y",
        _ => "text-sm font-normal text-gray-900 placeholder:text-gray-400"
    };

    private void TogglePasswordVisibility()
    {
        _isPasswordVisible = !_isPasswordVisible;
    }

    private async Task OpenPasswordGeneratorAsync()
    {
        var result = await ModalService.ShowAsync<PasswordGeneratorModal>(options: new AppModalOptions
        {
            Size = AppModalSize.Small,
            HasCloseButton = false
        });

        if (result.IsSuccess && result.Data is string newPassword && !string.IsNullOrWhiteSpace(newPassword))
        {
            Field.Value = newPassword;
            await OnFieldChanged.InvokeAsync();
            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task OnFileChangedAsync(StoredFileDto? file)
    {
        Field.File = file;
        await OnFieldChanged.InvokeAsync();
    }
}
