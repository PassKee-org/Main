using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Fluxor;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using PassKee.Web.Models.Vaults;
using PassKee.Web.Store.Vaults;
using PassKee.Web.Pages.App.Modals;
using PassKee.Api.Shared.Models.Vaults.Enums;
using PassKee.Api.Shared.Models.Vaults.Payloads;
using PassKee.Web.Store.Auth;

namespace PassKee.Web.Pages.App;

public partial class AppPage
{
    [Inject] public IState<VaultsState> VaultsState { get; set; } = null!;
    [Inject] public PassKee.Web.Services.Storage.ISessionLockStorageService SessionLockStorage { get; set; } = null!;

    private Guid? SelectedDirectoryId { get; set; }

    private IEnumerable<DecryptedCredential> FilteredCredentials =>
        SelectedDirectoryId.HasValue
            ? VaultsState.Value.Credentials.Where(c => c.DirectoryId == SelectedDirectoryId)
            : VaultsState.Value.Credentials;

    private IEnumerable<Guid?> VaultIds => VaultsState.Value.Vaults.Select(vault => (Guid?)vault.Id);

    private string GetVaultName(Guid? vaultId) =>
        VaultsState.Value.Vaults.FirstOrDefault(vault => vault.Id == vaultId)?.Name ?? string.Empty;

    protected override void OnInitialized()
    {
        base.OnInitialized();
        if (!AuthState.Value.IsAuthenticated || AuthState.Value.UserPrivateKey == null)
        {
            NavigationManager.NavigateTo("/login");
            return;
        }

        if (!VaultsState.Value.Vaults.Any() && !VaultsState.Value.IsLoading)
        {
            Dispatcher.Dispatch(new LoadVaultsAction());
        }
    }

    private string ActiveDirectoryName =>
        SelectedDirectoryId.HasValue
            ? VaultsState.Value.Directories.FirstOrDefault(d => d.Id == SelectedDirectoryId)?.Name ?? "Directory"
            : "All Items";

    protected bool IsMobileDrawerOpen { get; set; }
    protected void ToggleMobileDrawer() => IsMobileDrawerOpen = !IsMobileDrawerOpen;
    protected void CloseMobileDrawer() => IsMobileDrawerOpen = false;

    private void OnVaultSelected(Guid? vaultId)
    {
        if (vaultId.HasValue)
        {
            Dispatcher.Dispatch(new SelectVaultAction(vaultId.Value));
            Dispatcher.Dispatch(new LoadVaultDetailsAction(vaultId.Value));
            SelectedDirectoryId = null;
            IsMobileDrawerOpen = false;
        }
    }

    private async Task OpenCreateVaultModal()
    {
        var result = await ModalService.ShowAsync<CreateVaultModal>();
        if (result.IsSuccess && result.Data != null)
        {
            var vaultName = (string)result.Data;
            if (!string.IsNullOrWhiteSpace(vaultName))
            {
                Dispatcher.Dispatch(new CreateVaultAction(vaultName.Trim(), string.Empty));
            }
        }
    }

    private void SelectDirectory(Guid? id)
    {
        SelectedDirectoryId = id;
        IsMobileDrawerOpen = false;
    }

    private async Task OpenDirectoryModal(DecryptedDirectory? dir)
    {
        if (!VaultsState.Value.ActiveVaultId.HasValue) return;
        var vaultId = VaultsState.Value.ActiveVaultId.Value;

        var parameters = new Dictionary<string, object?>
        {
            { "Name", dir?.Name ?? string.Empty },
            { "IsEdit", dir != null }
        };

        var result = await ModalService.ShowAsync<EditDirectoryModal>(parameters);
        if (result.IsSuccess && result.Data != null)
        {
            if (dir == null)
            {
                var newName = (string)result.Data;
                Dispatcher.Dispatch(new CreateDirectoryAction(vaultId, SelectedDirectoryId, newName));
            }
            else
            {
                var newName = (string)result.Data;
                Dispatcher.Dispatch(new UpdateDirectoryAction(vaultId, dir.Id, dir.ParentDirectoryId, newName));
            }
        }
    }

    private async Task DeleteDirectory(DecryptedDirectory dir)
    {
        if (!VaultsState.Value.ActiveVaultId.HasValue) return;
        var vaultId = VaultsState.Value.ActiveVaultId.Value;

        var confirm = await ModalService.ShowConfirmationAsync($"Are you sure you want to delete directory '{dir.Name}'? All nested items will be lost.");
        if (confirm)
        {
            if (SelectedDirectoryId == dir.Id) SelectedDirectoryId = null;
            Dispatcher.Dispatch(new DeleteDirectoryAction(vaultId, dir.Id));
        }
    }

    private async Task OpenCredentialModal(DecryptedCredential? cred)
    {
        if (!VaultsState.Value.ActiveVaultId.HasValue) return;
        var vaultId = VaultsState.Value.ActiveVaultId.Value;

        var parameters = new Dictionary<string, object?>
        {
            { "IsEdit", cred != null },
            { "Type", cred != null ? (CredentialType)cred.Type : CredentialType.Login },
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
                Dispatcher.Dispatch(new CreateCredentialAction(vaultId, SelectedDirectoryId, form.Type, payload));
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

    private void HandleDirectoryMove((Guid SourceId, Guid? TargetId) moveAction)
    {
        if (!VaultsState.Value.ActiveVaultId.HasValue) return;
        var vaultId = VaultsState.Value.ActiveVaultId.Value;

        var dir = VaultsState.Value.Directories.FirstOrDefault(d => d.Id == moveAction.SourceId);
        if (dir != null)
        {
            Dispatcher.Dispatch(new UpdateDirectoryAction(vaultId, dir.Id, moveAction.TargetId, dir.Name));
        }
    }

    private void HandleRootDragOver(DragEventArgs e) { }
    
    private void HandleRootDrop(DragEventArgs e)
    {
        if (DragDropState.DraggedDirectoryId.HasValue)
        {
            HandleDirectoryMove((DragDropState.DraggedDirectoryId.Value, null));
        }
        DragDropState.DraggedDirectoryId = null;
    }

    private async Task LockVaultAsync()
    {
        if (AuthState.Value.UserPrivateKey is { } userPrivateKey)
        {
            CryptographicOperations.ZeroMemory(userPrivateKey);
        }
        if (VaultsState.Value.ActiveVaultKey is { } activeVaultKey)
        {
            CryptographicOperations.ZeroMemory(activeVaultKey);
        }
        Dispatcher.Dispatch(new ResetAuthStateAction());
        Dispatcher.Dispatch(new ResetVaultsStateAction());
        await SessionLockStorage.LockSessionAsync();
        NavigationManager.NavigateTo("/login");
    }
}
