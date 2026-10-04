using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fluxor;
using Microsoft.AspNetCore.Components;
using PassKee.Api.Shared.Models.Vaults.Payloads;
using PassKee.Business.Common.Constants;
using PassKee.Web.Components;
using PassKee.Web.Models.Vaults;
using PassKee.Web.Shared.Modals;
using PassKee.Web.Store.Vaults;

namespace PassKee.Web.Pages.App;

public partial class CredentialListBlock : BaseReactiveComponent
{
    [Inject] public IState<VaultsState> VaultsState { get; set; } = null!;

    [Parameter]
    public EventCallback OnToggleMobileDrawer { get; set; }

    private string ActiveDirectoryName =>
        VaultsState.Value.SelectedDirectoryId.HasValue
            ? VaultsState.Value.Directories.FirstOrDefault(d => d.Id == VaultsState.Value.SelectedDirectoryId)?.Name ?? "Directory"
            : "All Items";

    private IEnumerable<DecryptedCredential> Credentials =>
        VaultsState.Value.SelectedDirectoryId.HasValue
            ? VaultsState.Value.Credentials.Where(c => c.DirectoryId == VaultsState.Value.SelectedDirectoryId)
            : VaultsState.Value.Credentials;

    private async Task OpenCredentialModal(DecryptedCredential? cred)
    {
        if (!VaultsState.Value.ActiveVaultId.HasValue) return;
        var directoryId = cred == null ? VaultsState.Value.SelectedDirectoryId : cred.DirectoryId;

        var parameters = new Dictionary<string, object?>
        {
            { "IsEdit", cred != null },
            { "Type", cred != null ? cred.Type : CredentialType.Login },
            { "Title", cred?.Payload?.Title ?? string.Empty },
            { "Notes", cred?.Payload?.Notes ?? string.Empty },
            { "TagIds", cred?.Payload?.TagIds?.ToList() ?? new List<Guid>() },
            { "AdditionalFields", cred?.Payload?.AdditionalFields ?? [] },
            { "Sections", cred?.Payload?.Sections ?? [] },
            { "CredentialId", (Guid?)cred?.Id },
            { "DirectoryId", directoryId }
        };

        if (cred?.Payload is LoginCredentialPayload login)
        {
            parameters.Add("Username", login.Username);
            parameters.Add("Password", login.Password);
            parameters.Add("Website", login.Website);
        }
        else if (cred?.Payload is PasswordCredentialPayload pass)
        {
            parameters.Add("Username", pass.Username);
            parameters.Add("Password", pass.Password);
        }
        else if (cred?.Payload is CardCredentialPayload card)
        {
            parameters.Add("CardNumber", card.CardNumber);
            parameters.Add("CardholderName", card.CardholderName);
            parameters.Add("ExpirationDate", card.ExpirationDate);
            parameters.Add("Cvv", card.Cvv);
        }
        else if (cred?.Payload is FileCredentialPayload filePayload)
        {
            parameters.Add("File", filePayload.File);
        }

        await ModalService.ShowAsync<EditCredentialModal>(parameters, new PassKee.Web.Core.Services.UI.Modal.AppModalOptions
        {
            Size = PassKee.Web.Core.Services.UI.Modal.AppModalSize.Large,
            ModalClass = "!border-gray-200/80 !shadow-2xl"
        });
    }

    private async Task DeleteCredential(DecryptedCredential cred)
    {
        if (!VaultsState.Value.ActiveVaultId.HasValue) return;
        var vaultId = VaultsState.Value.ActiveVaultId.Value;

        var confirm = await ModalService.ShowConfirmationAsync($"Are you sure you want to delete '{cred.Payload.Title}'?");
        if (confirm)
        {
            Dispatcher.Dispatch(new DeleteCredentialAction(vaultId, cred.Id));
        }
    }
}
