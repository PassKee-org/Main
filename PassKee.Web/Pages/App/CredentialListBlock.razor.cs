using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fluxor;
using Microsoft.AspNetCore.Components;
using PassKee.Api.Shared.Models.Vaults.Payloads;
using PassKee.Business.Common.Constants;
using PassKee.Web.Components;
using PassKee.Web.Core.Services.Vaults;
using PassKee.Web.Core.Utils;
using PassKee.Web.Models.Vaults;
using PassKee.Web.Shared.Modals;
using PassKee.Web.Store.Vaults;

namespace PassKee.Web.Pages.App;

public partial class CredentialListBlock : BaseReactiveComponent
{
    [Inject] public IState<VaultsState> VaultsState { get; set; } = null!;
    [Inject] private IVaultSearchService VaultSearchService { get; set; } = null!;

    [Parameter]
    public EventCallback OnToggleMobileDrawer { get; set; }

    private Dictionary<Guid, string> _directoryPaths = new();
    private Dictionary<Guid, string> _tagNames = new();
    private Dictionary<Guid, List<DecryptedCredential>> _credentialsByDir = new();
    private List<DecryptedDirectory>? _lastDirectoriesRef;
    private List<DecryptedTag>? _lastTagsRef;
    private List<DecryptedCredential>? _lastCredentialsRef;

    private string? _cachedCredsQuery;
    private Guid? _cachedSelectedDirId;
    private bool _cachedIsArchiveSelected;
    private List<DecryptedCredential>? _cachedCredentialsRef;
    private ICollection<DecryptedCredential>? _cachedFilteredCredentials;

    private string? _cachedDirsQuery;
    private List<DecryptedDirectory>? _cachedDirsRef;
    private IReadOnlyList<DecryptedDirectory>? _cachedFilteredDirectories;

    private string SearchQuery => VaultsState.Value.SearchQuery;
    private bool IsSearchActive => !string.IsNullOrWhiteSpace(SearchQuery);

    private string ActiveDirectoryName =>
        VaultsState.Value.IsArchiveSelected
            ? "Archive"
            : VaultsState.Value.SelectedDirectoryId.HasValue
                ? VaultsState.Value.Directories.FirstOrDefault(d => d.Id == VaultsState.Value.SelectedDirectoryId)?.Name ?? "Directory"
                : "All Items";

    private ICollection<DecryptedCredential> FilteredCredentials
    {
        get
        {
            EnsureCaches();

            var query = SearchQuery?.Trim() ?? string.Empty;
            var isArchive = VaultsState.Value.IsArchiveSelected;
            var selectedDirId = isArchive ? null : VaultsState.Value.SelectedDirectoryId;
            var credentials = isArchive ? VaultsState.Value.ArchivedCredentials : VaultsState.Value.Credentials;

            if (ReferenceEquals(credentials, _cachedCredentialsRef) &&
                query == _cachedCredsQuery &&
                selectedDirId == _cachedSelectedDirId &&
                isArchive == _cachedIsArchiveSelected &&
                _cachedFilteredCredentials != null)
            {
                return _cachedFilteredCredentials;
            }

            _cachedCredentialsRef = credentials;
            _cachedCredsQuery = query;
            _cachedSelectedDirId = selectedDirId;
            _cachedIsArchiveSelected = isArchive;

            if (!string.IsNullOrWhiteSpace(query))
            {
                _cachedFilteredCredentials = VaultSearchService.Filter(credentials, query, _tagNames);
            }
            else if (selectedDirId.HasValue)
            {
                _cachedFilteredCredentials = _credentialsByDir.TryGetValue(selectedDirId.Value, out var list)
                    ? list
                    : [];
            }
            else
            {
                _cachedFilteredCredentials = credentials;
            }

            return _cachedFilteredCredentials;
        }
    }

    private IReadOnlyList<DecryptedDirectory> FilteredDirectories
    {
        get
        {
            if (!IsSearchActive || VaultsState.Value.IsArchiveSelected)
            {
                return [];
            }

            EnsureCaches();

            var query = SearchQuery?.Trim() ?? string.Empty;
            var dirs = VaultsState.Value.Directories;

            if (ReferenceEquals(dirs, _cachedDirsRef) &&
                query == _cachedDirsQuery &&
                _cachedFilteredDirectories != null)
            {
                return _cachedFilteredDirectories;
            }

            _cachedDirsRef = dirs;
            _cachedDirsQuery = query;
            _cachedFilteredDirectories = VaultSearchService.FilterDirectories(dirs, query, _directoryPaths);
            return _cachedFilteredDirectories;
        }
    }

    private int GetDirectoryItemCount(Guid dirId) =>
        _credentialsByDir.TryGetValue(dirId, out var list) ? list.Count : 0;

    private void EnsureCaches()
    {
        var state = VaultsState.Value;
        if (!ReferenceEquals(_lastDirectoriesRef, state.Directories))
        {
            _lastDirectoriesRef = state.Directories;
            _directoryPaths = VaultSearchService.BuildDirectoryPaths(state.Directories);
        }

        if (!ReferenceEquals(_lastTagsRef, state.Tags))
        {
            _lastTagsRef = state.Tags;
            _tagNames = state.Tags.ToDictionary(t => t.Id, t => t.Name);
        }

        if (!ReferenceEquals(_lastCredentialsRef, state.Credentials))
        {
            _lastCredentialsRef = state.Credentials;
            var byDir = new Dictionary<Guid, List<DecryptedCredential>>();
            foreach (var cred in state.Credentials)
            {
                if (cred.DirectoryId.HasValue)
                {
                    if (!byDir.TryGetValue(cred.DirectoryId.Value, out var list))
                    {
                        list = [];
                        byDir[cred.DirectoryId.Value] = list;
                    }
                    list.Add(cred);
                }
            }
            _credentialsByDir = byDir;
        }
    }

    private string GetDirectoryPath(Guid? directoryId)
    {
        if (directoryId.HasValue && _directoryPaths.TryGetValue(directoryId.Value, out var path))
        {
            return path;
        }
        return string.Empty;
    }

    private void HandleSelectDirectory(Guid? directoryId)
    {
        Dispatcher.Dispatch(new ClearSearchQueryAction());
        Dispatcher.Dispatch(new SelectDirectoryAction(directoryId));
    }

    private void ClearSearch()
    {
        Dispatcher.Dispatch(new ClearSearchQueryAction());
    }

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
            { "Icon", cred?.Payload?.Icon },
            { "TagIds", cred?.Payload?.TagIds?.ToList() ?? new List<Guid>() },
            { "AdditionalFields", cred?.Payload?.AdditionalFields ?? [] },
            { "Sections", cred?.Payload?.Sections ?? [] },
            { "CredentialId", (Guid?)cred?.Id },
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

        await ModalService.ShowAsync<EditCredentialModal>(parameters, new PassKee.Web.Core.Services.UI.Modal.AppModalOptions
        {
            Size = PassKee.Web.Core.Services.UI.Modal.AppModalSize.Large,
            ModalClass = "!border-gray-200/80 !shadow-2xl"
        });
    }

    private async Task OpenMoveModal(DecryptedCredential cred)
    {
        var parameters = new Dictionary<string, object?>
        {
            { "Credential", cred }
        };

        await ModalService.ShowAsync<MoveCredentialModal>(parameters, new PassKee.Web.Core.Services.UI.Modal.AppModalOptions
        {
            Size = PassKee.Web.Core.Services.UI.Modal.AppModalSize.Small
        });
    }

    private async Task DuplicateCredential(DecryptedCredential cred)
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

    private async Task ArchiveCredential(DecryptedCredential cred)
    {
        if (!VaultsState.Value.ActiveVaultId.HasValue) return;
        var vaultId = VaultsState.Value.ActiveVaultId.Value;

        var confirm = await ModalService.ShowConfirmationAsync($"Are you sure you want to archive '{cred.Payload.Title}'?");
        if (confirm)
        {
            Dispatcher.Dispatch(new ArchiveCredentialAction(vaultId, cred.Id));
        }
    }
}
