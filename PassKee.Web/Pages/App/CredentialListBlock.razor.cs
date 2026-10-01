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
using PassKee.Web.Pages.App.Modals;
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
        var vaultId = VaultsState.Value.ActiveVaultId.Value;

        var parameters = new Dictionary<string, object?>
        {
            { "IsEdit", cred != null },
            { "Type", cred != null ? cred.Type : CredentialType.Login },
            { "Title", cred?.Payload?.Title ?? string.Empty },
            { "Notes", cred?.Payload?.Notes ?? string.Empty }
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

        var result = await ModalService.ShowAsync<EditCredentialModal>(parameters);
        if (result.IsSuccess && result.Data != null)
        {
            var form = (CredentialModalResult)result.Data;
            BaseCredentialPayload payload;
            if (form.Type == CredentialType.Login)
                payload = new LoginCredentialPayload { Title = form.Title, Notes = form.Notes, Username = form.Username, Password = form.Password, Website = form.Website };
            else if (form.Type == CredentialType.Password)
                payload = new PasswordCredentialPayload { Title = form.Title, Notes = form.Notes, Username = form.Username, Password = form.Password };
            else if (form.Type == CredentialType.Card)
                payload = new CardCredentialPayload { Title = form.Title, Notes = form.Notes, CardNumber = form.CardNumber, CardholderName = form.CardholderName, ExpirationDate = form.ExpirationDate, Cvv = form.Cvv };
            else
                payload = new SecureNoteCredentialPayload { Title = form.Title, Notes = form.Notes };

            if (cred == null)
            {
                Dispatcher.Dispatch(new CreateCredentialAction(vaultId, VaultsState.Value.SelectedDirectoryId, form.Type, payload));
            }
            else
            {
                Dispatcher.Dispatch(new UpdateCredentialAction(vaultId, cred.Id, cred.DirectoryId, form.Type, payload));
            }
        }
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
