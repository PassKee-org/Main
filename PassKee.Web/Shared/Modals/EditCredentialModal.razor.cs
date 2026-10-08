using PassKee.Api.Shared.Models.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fluxor;
using Microsoft.AspNetCore.Components;
using PassKee.Api.Shared.Models.Vaults.Payloads;
using PassKee.Business.Common.Constants;
using PassKee.Web.Core.Services.UI.Modal;
using PassKee.Web.Core.Services.UI.Toast;
using PassKee.Web.Models.Vaults;
using PassKee.Web.Store.Vaults;

namespace PassKee.Web.Shared.Modals;

public partial class EditCredentialModal : ComponentBase
{
    [CascadingParameter] public AppModalInstance ModalInstance { get; set; } = null!;
    [Inject] public IAppModalDialogService ModalService { get; set; } = null!;
    [Inject] private IDispatcher Dispatcher { get; set; } = null!;
    [Inject] private IState<VaultsState> VaultsState { get; set; } = null!;
    [Inject] private IToastService ToastService { get; set; } = null!;

    [Parameter] public Guid? CredentialId { get; set; }
    [Parameter] public Guid? DirectoryId { get; set; }
    [Parameter] public bool IsEdit { get; set; }
    [Parameter] public bool StartInEditMode { get; set; }
    [Parameter] public CredentialType Type { get; set; } = CredentialType.Login;
    [Parameter] public string Title { get; set; } = string.Empty;
    [Parameter] public string? Notes { get; set; }
    [Parameter] public List<Guid> TagIds { get; set; } = [];
    [Parameter] public List<CredentialField> AdditionalFields { get; set; } = [];
    [Parameter] public List<CredentialSection> Sections { get; set; } = [];

    // Login / Password fields
    [Parameter] public string? Username { get; set; }
    [Parameter] public string? Password { get; set; }
    [Parameter] public string? Website { get; set; }

    // Card fields
    [Parameter] public string? CardNumber { get; set; }
    [Parameter] public string? CardholderName { get; set; }
    [Parameter] public string? ExpirationDate { get; set; }
    [Parameter] public string? Cvv { get; set; }

    // File fields
    [Parameter] public StoredFileDto? File { get; set; }

    private bool _isEditing;
    private bool _isExisting;
    private bool _isSaving;
    private Guid? _credentialVaultId;
    private Guid? _credentialId;
    private Guid? _directoryId;
    private List<Guid> _tagIds = [];
    private List<CredentialField> _additionalFields = [];
    private List<CredentialSection> _sections = [];
    private bool _isSaveAttempted;

    // Snapshot state for reverting when canceling an edit of existing credential
    private string _initialTitle = string.Empty;
    private string? _initialNotes;
    private List<Guid> _initialTagIds = [];
    private string? _initialUsername;
    private string? _initialPassword;
    private string? _initialWebsite;
    private string? _initialCardNumber;
    private string? _initialCardholderName;
    private string? _initialExpirationDate;
    private string? _initialCvv;
    private StoredFileDto? _initialFile;
    private List<CredentialField> _initialAdditionalFields = [];
    private List<CredentialSection> _initialSections = [];

    private string? TitleErrorMessage => _isSaveAttempted && string.IsNullOrWhiteSpace(Title)
        ? "Title is required."
        : null;

    protected override void OnInitialized()
    {
        _credentialVaultId = VaultsState.Value.ActiveVaultId;
        _credentialId = CredentialId;
        _directoryId = DirectoryId;
        _isExisting = IsEdit || CredentialId.HasValue;
        _isEditing = !_isExisting || StartInEditMode;

        _tagIds = TagIds.ToList();
        _additionalFields = AdditionalFields.Select(CloneField).ToList();
        _sections = Sections.Select(CloneSection).ToList();

        CaptureSnapshot();
    }

    private void CaptureSnapshot()
    {
        _initialTitle = Title;
        _initialNotes = Notes;
        _initialTagIds = _tagIds.ToList();
        _initialUsername = Username;
        _initialPassword = Password;
        _initialWebsite = Website;
        _initialCardNumber = CardNumber;
        _initialCardholderName = CardholderName;
        _initialExpirationDate = ExpirationDate;
        _initialCvv = Cvv;
        _initialFile = File;
        _initialAdditionalFields = AdditionalFields.Select(CloneField).ToList();
        _initialSections = Sections.Select(CloneSection).ToList();
    }

    private static CredentialField CloneField(CredentialField field) => new()
    {
        Type = field.Type,
        Label = field.Label,
        Value = field.Value,
        File = field.File
    };

    private static CredentialSection CloneSection(CredentialSection section) => new()
    {
        Title = section.Title,
        Fields = section.Fields.Select(CloneField).ToList()
    };

    private void EnterEditMode()
    {
        _isEditing = true;
    }

    private void CancelEdit()
    {
        if (_isSaving) return;

        if (!_isExisting)
        {
            Cancel();
            return;
        }

        // Revert to initial snapshot
        Title = _initialTitle;
        Notes = _initialNotes;
        _tagIds = _initialTagIds.ToList();
        Username = _initialUsername;
        Password = _initialPassword;
        Website = _initialWebsite;
        CardNumber = _initialCardNumber;
        CardholderName = _initialCardholderName;
        ExpirationDate = _initialExpirationDate;
        Cvv = _initialCvv;
        File = _initialFile;
        _additionalFields = _initialAdditionalFields.Select(CloneField).ToList();
        _sections = _initialSections.Select(CloneSection).ToList();
        _isSaveAttempted = false;
        _isEditing = false;
    }

    private void AddField(CredentialFieldType type)
    {
        var newField = new CredentialField
        {
            Type = type,
            Label = Parts.CredentialFieldMenuBlock.GetFieldLabel(type)
        };

        if (_sections.Count > 0)
        {
            _sections[^1].Fields.Add(newField);
        }
        else
        {
            _additionalFields.Add(newField);
        }
    }

    private void AddSection()
    {
        _sections.Add(new CredentialSection { Title = "New section" });
    }

    private void RemoveSection(CredentialSection section)
    {
        _sections.Remove(section);
    }

    private void HandleCredentialFieldChanged()
    {
        StateHasChanged();
    }

    private void OnTypeChanged(CredentialType? newType)
    {
        if (newType.HasValue)
        {
            var oldType = Type;
            Type = newType.Value;

            if (!_isExisting)
            {
                ApplyTemplateDefaults(Type, oldType);
            }
        }
    }

    private void ApplyTemplateDefaults(CredentialType newType, CredentialType oldType)
    {
        // If user switches type on a fresh new item, preload default structure
        if (newType == CredentialType.Database)
        {
            _additionalFields =
            [
                new CredentialField { Type = CredentialFieldType.Text, Label = "Type" },
                new CredentialField { Type = CredentialFieldType.Text, Label = "Server" },
                new CredentialField { Type = CredentialFieldType.Text, Label = "Port" },
                new CredentialField { Type = CredentialFieldType.Text, Label = "Database" },
                new CredentialField { Type = CredentialFieldType.Text, Label = "Username" },
                new CredentialField { Type = CredentialFieldType.Password, Label = "Password" },
                new CredentialField { Type = CredentialFieldType.Text, Label = "SID" },
                new CredentialField { Type = CredentialFieldType.Text, Label = "Alias" },
                new CredentialField { Type = CredentialFieldType.Text, Label = "Connection options" }
            ];
            _sections = [];
        }
        else if (newType == CredentialType.Server)
        {
            _additionalFields = [];
            _sections =
            [
                new CredentialSection
                {
                    Title = "Admin console",
                    Fields =
                    [
                        new CredentialField { Type = CredentialFieldType.Url, Label = "Admin console URL" },
                        new CredentialField { Type = CredentialFieldType.Text, Label = "Admin console username" },
                        new CredentialField { Type = CredentialFieldType.Password, Label = "Console password" }
                    ]
                },
                new CredentialSection
                {
                    Title = "Hosting provider",
                    Fields =
                    [
                        new CredentialField { Type = CredentialFieldType.Text, Label = "Name" },
                        new CredentialField { Type = CredentialFieldType.Url, Label = "Website" },
                        new CredentialField { Type = CredentialFieldType.Url, Label = "Support URL" },
                        new CredentialField { Type = CredentialFieldType.Phone, Label = "Support phone" }
                    ]
                }
            ];
        }
        else if (oldType is CredentialType.Database or CredentialType.Server)
        {
            _additionalFields = [];
            _sections = [];
        }
    }

    private static string GetCredentialTypeIcon(CredentialType type) => type switch
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

    private static string GetCredentialTypeLabel(CredentialType type) => type switch
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

    private async Task OpenPasswordGeneratorForMainPasswordAsync()
    {
        var result = await ModalService.ShowAsync<PasswordGeneratorModal>(options: new AppModalOptions
        {
            Size = AppModalSize.Small,
            HasCloseButton = false
        });

        if (result.IsSuccess && result.Data is string newPassword && !string.IsNullOrWhiteSpace(newPassword))
        {
            Password = newPassword;
            StateHasChanged();
        }
    }

    private IEnumerable<DecryptedTag> SelectedTags =>
        VaultsState.Value.Tags.Where(t => _tagIds.Contains(t.Id));

    private void OnTagsChanged(IEnumerable<Guid> ids)
    {
        _tagIds = ids.ToList();
    }

    private async Task<DecryptedTag?> CreateTagAsync(string name)
    {
        var activeVaultId = VaultsState.Value.ActiveVaultId;
        if (!activeVaultId.HasValue)
        {
            ToastService.ShowError("Vault is locked or not selected.");
            return null;
        }

        var completion = new TaskCompletionSource<DecryptedTag?>(TaskCreationOptions.RunContinuationsAsynchronously);
        Dispatcher.Dispatch(new CreateTagAction(activeVaultId.Value, name, completion));
        return await completion.Task;
    }

    private void Cancel()
    {
        if (_isSaving) return;

        ModalService.Close(ModalInstance, AppModalResult.Cancel());
    }

    private Task Save() => SaveAsync(closeAfterSave: false);

    private Task SaveAndClose() => SaveAsync(closeAfterSave: true);

    private async Task SaveAsync(bool closeAfterSave)
    {
        if (_isSaving) return;

        _isSaveAttempted = true;
        if (string.IsNullOrWhiteSpace(Title)) return;

        if (VaultsState.Value.PendingFileUploads > 0)
        {
            ToastService.ShowWarning("Wait for the file upload to finish.");
            return;
        }

        var activeVaultId = VaultsState.Value.ActiveVaultId;
        if (!activeVaultId.HasValue)
        {
            ToastService.ShowError("Vault is locked or not selected.");
            return;
        }

        if (_credentialId.HasValue && _credentialVaultId != activeVaultId)
        {
            ToastService.ShowError("The active vault changed. Close and reopen this credential before saving.");
            return;
        }

        var result = new CredentialModalResult
        {
            Type = Type,
            Title = Title,
            Notes = Notes,
            TagIds = _tagIds.ToList(),
            AdditionalFields = _additionalFields,
            Sections = _sections,
            Username = Username,
            Password = Password,
            Website = Website,
            CardNumber = CardNumber,
            CardholderName = CardholderName,
            ExpirationDate = ExpirationDate,
            Cvv = Cvv,
            File = File
        };

        var payload = BuildCredentialPayload(result);
        var requestId = Guid.NewGuid();
        var isCreate = !_credentialId.HasValue;
        var completion = new TaskCompletionSource<CredentialSaveResult>(TaskCreationOptions.RunContinuationsAsynchronously);
        _isSaving = true;

        CredentialSaveResult saveResult;
        try
        {
            if (isCreate)
            {
                Dispatcher.Dispatch(new CreateCredentialAction(requestId, result.Type, payload, completion, _directoryId));
            }
            else
            {
                Dispatcher.Dispatch(new UpdateCredentialAction(requestId, _credentialId!.Value, _directoryId, result.Type, payload, completion));
            }

            saveResult = await completion.Task;
        }
        catch
        {
            ToastService.ShowError(isCreate ? "Error creating credential" : "Error updating credential");
            return;
        }
        finally
        {
            _isSaving = false;
        }

        if (!saveResult.IsSuccess || saveResult.RequestId != requestId)
        {
            ToastService.ShowError(saveResult.ErrorMessage ?? "Error saving credential");
            return;
        }

        _credentialId = saveResult.CredentialId;
        _credentialVaultId = saveResult.VaultId;
        _directoryId = saveResult.DirectoryId;
        _isExisting = true;
        _isEditing = false;
        _isSaveAttempted = false;
        CaptureSnapshot();
        ToastService.ShowSuccess(isCreate ? "Credential created" : "Credential updated");

        if (closeAfterSave)
        {
            ModalService.Close(ModalInstance, AppModalResult.Ok());
        }
    }

    private static BaseCredentialPayload BuildCredentialPayload(CredentialModalResult form)
    {
        BaseCredentialPayload payload = form.Type switch
        {
            CredentialType.Login => new LoginCredentialPayload { Title = form.Title, Notes = form.Notes, Username = form.Username, Password = form.Password, Website = form.Website },
            CredentialType.Password => new PasswordCredentialPayload { Title = form.Title, Notes = form.Notes, Username = form.Username, Password = form.Password },
            CredentialType.Card => new CardCredentialPayload { Title = form.Title, Notes = form.Notes, CardNumber = form.CardNumber, CardholderName = form.CardholderName, ExpirationDate = form.ExpirationDate, Cvv = form.Cvv },
            CredentialType.File => new FileCredentialPayload { Title = form.Title, Notes = form.Notes, File = form.File },
            CredentialType.Database => new DatabaseCredentialPayload { Title = form.Title, Notes = form.Notes },
            CredentialType.SshKey => new SshKeyCredentialPayload { Title = form.Title, Notes = form.Notes, PrivateKeyFile = form.File },
            CredentialType.Server => new ServerCredentialPayload { Title = form.Title, Notes = form.Notes, Url = form.Website, Username = form.Username, Password = form.Password },
            _ => new SecureNoteCredentialPayload { Title = form.Title, Notes = form.Notes }
        };

        payload.TagIds = form.TagIds;
        payload.AdditionalFields = form.AdditionalFields;
        payload.Sections = form.Sections;
        return payload;
    }
}

public class CredentialModalResult
{
    public CredentialType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public List<Guid> TagIds { get; set; } = [];
    public List<CredentialField> AdditionalFields { get; set; } = [];
    public List<CredentialSection> Sections { get; set; } = [];
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string? Website { get; set; }
    public string? CardNumber { get; set; }
    public string? CardholderName { get; set; }
    public string? ExpirationDate { get; set; }
    public string? Cvv { get; set; }
    public StoredFileDto? File { get; set; }
}
