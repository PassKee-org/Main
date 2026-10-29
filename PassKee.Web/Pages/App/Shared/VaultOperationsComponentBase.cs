using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fluxor;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using PassKee.Api.Shared.Models.Vaults.Payloads;
using PassKee.Business.Common.Constants;
using PassKee.Web.Components;
using PassKee.Web.Constants;
using PassKee.Web.Constants.Navigation;
using PassKee.Web.Core.Services.UI.Modal;
using PassKee.Web.Core.Services.Vaults;
using PassKee.Web.Core.Utils;
using PassKee.Web.Models.Vaults;
using PassKee.Web.Services.Auth;
using PassKee.Web.Services.Storage;
using PassKee.Web.Services.UI.Viewport;
using PassKee.Web.Shared.Modals;
using PassKee.Web.Store.Export;
using PassKee.Web.Store.Import;
using PassKee.Web.Store.Ui;
using PassKee.Web.Store.Vaults;
using PassKee.Web.Utils;

namespace PassKee.Web.Pages.App.Shared;

public abstract class VaultOperationsComponentBase : BaseReactiveComponent
{
    [Inject] public IState<VaultsState> VaultsState { get; set; } = null!;
    [Inject] public IState<UiState> UiState { get; set; } = null!;
    [Inject] public IState<ImportState> ImportState { get; set; } = null!;
    [Inject] public IState<ExportState> ExportState { get; set; } = null!;
    [Inject] protected IVaultSearchService VaultSearchService { get; set; } = null!;
    [Inject] protected ISessionLockStorageService SessionLockStorage { get; set; } = null!;
    [Inject] protected IInactivityLockService InactivityLockService { get; set; } = null!;

    private List<DecryptedDirectory>? _lastDirsRef;
    private List<DecryptedCredential>? _lastCredsRef;
    private List<DecryptedTag>? _lastTagsRef;
    private VaultHierarchySnapshot? _hierarchySnapshot;

    protected VaultHierarchySnapshot Hierarchy
    {
        get
        {
            var state = VaultsState.Value;
            if (_hierarchySnapshot == null ||
                !ReferenceEquals(_lastDirsRef, state.Directories) ||
                !ReferenceEquals(_lastCredsRef, state.Credentials) ||
                !ReferenceEquals(_lastTagsRef, state.Tags))
            {
                _lastDirsRef = state.Directories;
                _lastCredsRef = state.Credentials;
                _lastTagsRef = state.Tags;
                _hierarchySnapshot = new VaultHierarchySnapshot(
                    state.Directories,
                    state.Credentials,
                    state.Tags,
                    VaultSearchService);
            }

            return _hierarchySnapshot;
        }
    }

    protected bool CanImportOrExport =>
        VaultsState.Value.ActiveVaultId.HasValue &&
        VaultsState.Value.ActiveVaultKey != null &&
        !VaultsState.Value.IsLoading &&
        !ImportState.Value.IsRunning &&
        !ExportState.Value.IsRunning;

    protected bool CanDeleteVault =>
        VaultsState.Value.ActiveVaultId.HasValue &&
        !VaultsState.Value.IsLoading &&
        !ImportState.Value.IsRunning &&
        !ExportState.Value.IsRunning;

    protected IEnumerable<Guid?> VaultIds =>
        VaultsState.Value.Vaults.Select(vault => (Guid?)vault.Id);

    protected string GetVaultName(Guid? vaultId) =>
        VaultsState.Value.Vaults.FirstOrDefault(vault => vault.Id == vaultId)?.Name ?? string.Empty;

    protected string ActiveVaultName =>
        GetVaultName(VaultsState.Value.ActiveVaultId);

    protected DecryptedDirectory? SelectedDirectory =>
        !VaultsState.Value.IsArchiveSelected && VaultsState.Value.SelectedDirectoryId.HasValue &&
        Hierarchy.DirectoryMap.TryGetValue(VaultsState.Value.SelectedDirectoryId.Value, out var dir)
            ? dir
            : null;

    protected string ActiveDirectoryName =>
        VaultsState.Value.IsArchiveSelected
            ? "Archive"
            : SelectedDirectory?.Name ?? "No Directory";

    protected string? ActiveDirectoryPath =>
        VaultsState.Value.IsArchiveSelected || !VaultsState.Value.SelectedDirectoryId.HasValue
            ? null
            : Hierarchy.GetDirectoryPath(VaultsState.Value.SelectedDirectoryId);

    protected string SearchQuery => VaultsState.Value.SearchQuery;
    protected bool IsSearchActive => !string.IsNullOrWhiteSpace(SearchQuery);

    protected void SelectVault(Guid? vaultId)
    {
        if (vaultId.HasValue && vaultId != VaultsState.Value.ActiveVaultId)
        {
            Dispatcher.Dispatch(new SelectVaultAction(vaultId.Value));
            Dispatcher.Dispatch(new LoadVaultDetailsAction(vaultId.Value));
        }
    }

    protected async Task OpenCreateVaultModalAsync()
    {
        var result = await ModalService.ShowAsync<CreateVaultModal>();
        if (result.IsSuccess && result.Data is string vaultName && !string.IsNullOrWhiteSpace(vaultName))
        {
            Dispatcher.Dispatch(new CreateVaultAction(vaultName.Trim(), string.Empty));
        }
    }

    protected async Task OpenImportModalAsync()
    {
        if (!CanImportOrExport) return;
        await ModalService.ShowAsync<KdbxImportModal>(options: new AppModalOptions
        {
            Size = AppModalSize.Medium,
            IsScrollable = true,
            HasCloseButton = false,
            IsCloseOnBackdropClick = false,
            IsCloseOnEscapeKey = false
        });
    }

    protected async Task OpenExportModalAsync()
    {
        if (!CanImportOrExport) return;
        await ModalService.ShowAsync<KdbxExportModal>(options: new AppModalOptions
        {
            Size = AppModalSize.Medium,
            IsScrollable = true,
            HasCloseButton = false,
            IsCloseOnBackdropClick = false,
            IsCloseOnEscapeKey = false
        });
    }

    protected async Task DeleteActiveVaultAsync()
    {
        if (!CanDeleteVault) return;
        var vaultId = VaultsState.Value.ActiveVaultId!.Value;
        var vaultName = GetVaultName(vaultId);

        var confirm = await ModalService.ShowConfirmationAsync(
            $"Are you sure you want to delete vault '{vaultName}'? All entries and attachments will be deleted. This action cannot be undone.",
            title: "Delete Vault",
            confirmText: "Delete",
            cancelText: "Cancel",
            type: AppConfirmationType.Danger);

        if (confirm)
        {
            Dispatcher.Dispatch(new DeleteVaultAction(vaultId));
        }
    }

    protected async Task LockVaultAsync()
    {
        await InactivityLockService.LockAsync();
    }

    protected async Task LogoutAsync()
    {
        await ApiService.LogoutAsync();
        await SessionLockStorage.ClearAllSessionDataAsync();
        await LockVaultAsync();
    }

    protected async Task OpenPasswordGeneratorAsync()
    {
        var parameters = new Dictionary<string, object?>
        {
            { nameof(PasswordGeneratorModal.IsStandalone), true }
        };

        await ModalService.ShowAsync<PasswordGeneratorModal>(parameters, options: new AppModalOptions
        {
            Size = AppModalSize.Small,
            HasCloseButton = false
        });
    }

    protected async Task OpenGuidGeneratorAsync()
    {
        await ModalService.ShowAsync<GuidGeneratorModal>(options: new AppModalOptions
        {
            Size = AppModalSize.Small,
            HasCloseButton = false
        });
    }

    protected void SelectDirectory(Guid? directoryId)
    {
        Dispatcher.Dispatch(new SelectDirectoryAction(directoryId));
    }

    protected void SelectDirectoryFromSearch(Guid? directoryId)
    {
        Dispatcher.Dispatch(new ClearSearchQueryAction());
        Dispatcher.Dispatch(new SelectDirectoryAction(directoryId));
        if (UiState.Value.ActiveMobileTab != MobileNavigationTab.Groups)
        {
            Dispatcher.Dispatch(new SetMobileNavigationTabAction(MobileNavigationTab.Groups));
        }
    }

    protected void NavigateUpDirectory()
    {
        if (VaultsState.Value.IsArchiveSelected)
        {
            Dispatcher.Dispatch(new SelectDirectoryAction(null));
            return;
        }

        if (VaultsState.Value.SelectedDirectoryId.HasValue)
        {
            var parentId = Hierarchy.GetParentDirectoryId(VaultsState.Value.SelectedDirectoryId);
            Dispatcher.Dispatch(new SelectDirectoryAction(parentId));
        }
    }

    protected void SelectArchive()
    {
        var vaultId = VaultsState.Value.ActiveVaultId;
        if (vaultId.HasValue)
        {
            Dispatcher.Dispatch(new SelectArchiveAction());
            Dispatcher.Dispatch(new LoadArchivedCredentialsAction(vaultId.Value));
        }
    }

    protected void ClearSearch()
    {
        Dispatcher.Dispatch(new ClearSearchQueryAction());
    }

    protected Task OpenCreateDirectoryModalAsync(Guid? parentDirectoryId = null) =>
        OpenDirectoryModalInternalAsync(null, parentDirectoryId);

    protected Task OpenCreateSubDirectoryModalAsync(DecryptedDirectory parentDir) =>
        OpenDirectoryModalInternalAsync(null, parentDir.Id);

    protected Task OpenEditDirectoryModalAsync(DecryptedDirectory? dir) =>
        OpenDirectoryModalInternalAsync(dir, null);

    private async Task OpenDirectoryModalInternalAsync(DecryptedDirectory? dir, Guid? explicitParentDirectoryId)
    {
        if (!VaultsState.Value.ActiveVaultId.HasValue) return;
        var vaultId = VaultsState.Value.ActiveVaultId.Value;

        Guid? parentId = dir != null
            ? dir.ParentDirectoryId
            : (explicitParentDirectoryId ?? VaultsState.Value.SelectedDirectoryId);

        string? parentName = null;
        if (parentId.HasValue && Hierarchy.DirectoryMap.TryGetValue(parentId.Value, out var parentDir))
        {
            parentName = parentDir.Name;
        }

        var parameters = new Dictionary<string, object?>
        {
            { "Name", dir?.Name ?? string.Empty },
            { "IsEdit", dir != null },
            { "ParentDirectoryName", parentName }
        };

        var result = await ModalService.ShowAsync<EditDirectoryModal>(parameters);
        if (result.IsSuccess && result.Data is string newName && !string.IsNullOrWhiteSpace(newName))
        {
            if (dir == null)
            {
                Dispatcher.Dispatch(new CreateDirectoryAction(vaultId, parentId, newName));
            }
            else
            {
                Dispatcher.Dispatch(new UpdateDirectoryAction(vaultId, dir.Id, dir.ParentDirectoryId, newName));
            }
        }
    }

    protected async Task DeleteDirectoryAsync(DecryptedDirectory dir)
    {
        if (!VaultsState.Value.ActiveVaultId.HasValue) return;
        var vaultId = VaultsState.Value.ActiveVaultId.Value;

        var confirm = await ModalService.ShowConfirmationAsync(
            $"Are you sure you want to delete directory '{dir.Name}'? All nested items will be archived.");
        if (confirm)
        {
            Dispatcher.Dispatch(new DeleteDirectoryAction(vaultId, dir.Id));
        }
    }

    protected async Task OpenMoveDirectoryModalAsync(DecryptedDirectory dir)
    {
        var parameters = new Dictionary<string, object?>
        {
            { "Directory", dir }
        };

        await ModalService.ShowAsync<MoveDirectoryModal>(parameters, new AppModalOptions
        {
            Size = AppModalSize.Small
        });
    }

    protected void HandleDirectoryMove((Guid SourceId, Guid? TargetId) moveAction)
    {
        if (!VaultsState.Value.ActiveVaultId.HasValue) return;
        var vaultId = VaultsState.Value.ActiveVaultId.Value;

        if (Hierarchy.DirectoryMap.TryGetValue(moveAction.SourceId, out var dir))
        {
            Dispatcher.Dispatch(new MoveDirectoryAction(Guid.NewGuid(), vaultId, dir.Id, moveAction.TargetId));
        }
    }

    protected Task OpenViewCredentialModalAsync(DecryptedCredential cred) =>
        OpenCredentialModalAsync(cred, startInEditMode: false);

    protected Task OpenEditCredentialModalAsync(DecryptedCredential? cred) =>
        OpenCredentialModalAsync(cred, startInEditMode: true);

    protected async Task OpenCredentialModalAsync(
        DecryptedCredential? cred,
        bool startInEditMode = false,
        CredentialType defaultType = CredentialType.Login)
    {
        if (!VaultsState.Value.ActiveVaultId.HasValue) return;
        var directoryId = cred == null ? VaultsState.Value.SelectedDirectoryId : cred.DirectoryId;

        var parameters = new Dictionary<string, object?>
        {
            { "IsEdit", cred != null },
            { "StartInEditMode", startInEditMode },
            { "Type", cred?.Type ?? defaultType },
            { "Title", cred?.Payload?.Title ?? string.Empty },
            { "Notes", cred?.Payload?.Notes ?? string.Empty },
            { "Icon", cred?.Payload?.Icon },
            { "TagIds", cred?.Payload?.TagIds?.ToList() ?? new List<Guid>() },
            { "AdditionalFields", cred?.Payload?.AdditionalFields ?? [] },
            { "Sections", cred?.Payload?.Sections ?? [] },
            { "CredentialId", cred?.Id },
            { "DirectoryId", directoryId },
            { "IsArchived", cred?.ArchivedAt != null || VaultsState.Value.IsArchiveSelected }
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
        else if (cred?.Payload is SshKeyCredentialPayload sshKey)
        {
            parameters.Add("File", sshKey.PrivateKeyFile);
        }
        else if (cred?.Payload is ServerCredentialPayload server)
        {
            parameters.Add("Website", server.Url);
            parameters.Add("Username", server.Username);
            parameters.Add("Password", server.Password);
        }

        await ModalService.ShowAsync<EditCredentialModal>(parameters, new AppModalOptions
        {
            Size = AppModalSize.Large,
            ModalClass = "!border-gray-200/80 !shadow-2xl"
        });
    }

    protected async Task OpenMoveCredentialModalAsync(DecryptedCredential cred)
    {
        var parameters = new Dictionary<string, object?>
        {
            { "Credential", cred }
        };

        await ModalService.ShowAsync<MoveCredentialModal>(parameters, new AppModalOptions
        {
            Size = AppModalSize.Small
        });
    }

    protected async Task OpenShareCredentialModalAsync(DecryptedCredential cred)
    {
        var parameters = new Dictionary<string, object?>
        {
            { "Credential", cred }
        };

        await ModalService.ShowAsync<ShareCredentialModal>(parameters, new AppModalOptions
        {
            Size = AppModalSize.Medium
        });
    }

    protected async Task DuplicateCredentialAsync(DecryptedCredential cred)
    {
        var activeVaultId = VaultsState.Value.ActiveVaultId;
        if (!activeVaultId.HasValue)
        {
            ToastService.ShowError("Vault is locked or not selected.");
            return;
        }

        var clonedPayload = CredentialCloner.CloneWithCopyTitle(cred.Payload);
        var requestId = Guid.NewGuid();
        var completion = new TaskCompletionSource<CredentialSaveResult>(TaskCreationOptions.RunContinuationsAsynchronously);

        try
        {
            Dispatcher.Dispatch(new CreateCredentialAction(requestId, cred.Type, clonedPayload, completion, cred.DirectoryId));
            var result = await completion.Task;
            if (result.IsSuccess)
            {
                ToastService.ShowSuccess($"\"{clonedPayload.Title}\" created");
            }
            else
            {
                ToastService.ShowError(result.ErrorMessage ?? "Error duplicating credential");
            }
        }
        catch
        {
            ToastService.ShowError("Error duplicating credential");
        }
    }

    protected async Task ArchiveCredentialAsync(DecryptedCredential cred)
    {
        if (!VaultsState.Value.ActiveVaultId.HasValue) return;
        var vaultId = VaultsState.Value.ActiveVaultId.Value;

        var confirm = await ModalService.ShowConfirmationAsync($"Are you sure you want to archive '{cred.Payload?.Title}'?");
        if (confirm)
        {
            Dispatcher.Dispatch(new ArchiveCredentialAction(vaultId, cred.Id));
        }
    }

    protected async Task DeleteSingleArchivedCredentialAsync(DecryptedCredential cred)
    {
        if (!VaultsState.Value.ActiveVaultId.HasValue) return;
        var vaultId = VaultsState.Value.ActiveVaultId.Value;

        var confirm = await ModalService.ShowConfirmationAsync(
            $"Are you sure you want to permanently delete '{cred.Payload?.Title}'? This action cannot be undone.");
        if (confirm)
        {
            Dispatcher.Dispatch(new DeleteCredentialAction(vaultId, cred.Id));
        }
    }

    protected async Task CopyToClipboardAsync(string? value, string label)
    {
        if (string.IsNullOrEmpty(value))
        {
            return;
        }

        try
        {
            await Js.InvokeVoidAsync("PassKeeInterop.copyToClipboard", value, 60);
            ToastService.ShowSuccess($"{label} copied to clipboard");
        }
        catch
        {
            ToastService.ShowError("Could not copy to clipboard");
        }
    }

    public static (string? Value, string Label) GetPrimaryCopyableField(DecryptedCredential cred) => cred.Payload switch
    {
        LoginCredentialPayload { Password: { Length: > 0 } pwd } => (pwd, "Password"),
        PasswordCredentialPayload { Password: { Length: > 0 } pwd } => (pwd, "Password"),
        ServerCredentialPayload { Password: { Length: > 0 } pwd } => (pwd, "Password"),
        CardCredentialPayload { CardNumber: { Length: > 0 } num } => (num, "Card number"),
        SecureNoteCredentialPayload { Notes: { Length: > 0 } notes } => (notes, "Note"),
        _ => (null, string.Empty)
    };

    public static (string? Value, string Label) GetSecondaryCopyableField(DecryptedCredential cred) => cred.Payload switch
    {
        LoginCredentialPayload { Username: { Length: > 0 } user } => (user, "Username"),
        PasswordCredentialPayload { Username: { Length: > 0 } user } => (user, "Username"),
        ServerCredentialPayload { Username: { Length: > 0 } user } => (user, "Username"),
        CardCredentialPayload { Cvv: { Length: > 0 } cvv } => (cvv, "CVV"),
        _ => (null, string.Empty)
    };

    public static string? GetCredentialSubtitle(DecryptedCredential cred) => cred.Payload switch
    {
        LoginCredentialPayload login when !string.IsNullOrWhiteSpace(login.Username) => login.Username,
        LoginCredentialPayload login when !string.IsNullOrWhiteSpace(login.Website) => login.Website,
        PasswordCredentialPayload pass when !string.IsNullOrWhiteSpace(pass.Username) => pass.Username,
        ServerCredentialPayload server when !string.IsNullOrWhiteSpace(server.Username) => server.Username,
        ServerCredentialPayload server when !string.IsNullOrWhiteSpace(server.Url) => server.Url,
        CardCredentialPayload card when !string.IsNullOrWhiteSpace(card.CardholderName) => card.CardholderName,
        FileCredentialPayload { File.FileName: { Length: > 0 } fileName } => fileName,
        SshKeyCredentialPayload { PrivateKeyFile.FileName: { Length: > 0 } keyName } => keyName,
        _ => null
    };

    public static string GetCredentialTypeIcon(CredentialType type) => type switch
    {
        CredentialType.Login => "fa-solid fa-globe",
        CredentialType.Password => "fa-solid fa-key",
        CredentialType.Card => "fa-solid fa-credit-card",
        CredentialType.SecureNote => "fa-solid fa-note-sticky",
        CredentialType.File => "fa-solid fa-file",
        CredentialType.Database => "fa-solid fa-database",
        CredentialType.SshKey => "fa-solid fa-terminal",
        CredentialType.Server => "fa-solid fa-server",
        _ => "fa-solid fa-shield-halved"
    };

    public static string GetCredentialTypeLabel(CredentialType type) => type switch
    {
        CredentialType.Login => "Login",
        CredentialType.Password => "Password",
        CredentialType.Card => "Card",
        CredentialType.SecureNote => "Secure Note",
        CredentialType.File => "File",
        CredentialType.Database => "Database",
        CredentialType.SshKey => "SSH Key",
        CredentialType.Server => "Server",
        _ => "Item"
    };
}

