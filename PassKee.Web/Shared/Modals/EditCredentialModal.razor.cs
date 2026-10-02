using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using PassKee.Api.Shared.Models.Vaults.Payloads;
using PassKee.Business.Common.Constants;
using PassKee.Web.Core.Services.UI.Modal;

namespace PassKee.Web.Shared.Modals;

public partial class EditCredentialModal : ComponentBase
{
    [CascadingParameter] public AppModalInstance ModalInstance { get; set; } = null!;
    [Inject] public IAppModalDialogService ModalService { get; set; } = null!;

    [Parameter] public bool IsEdit { get; set; }
    [Parameter] public bool StartInEditMode { get; set; }
    [Parameter] public CredentialType Type { get; set; } = CredentialType.Login;
    [Parameter] public string Title { get; set; } = string.Empty;
    [Parameter] public string? Notes { get; set; }
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

    private bool _isEditing;
    private List<CredentialField> _additionalFields = [];
    private List<CredentialSection> _sections = [];
    private bool _isSaveAttempted;

    // Snapshot state for reverting when canceling an edit of existing credential
    private string _initialTitle = string.Empty;
    private string? _initialNotes;
    private string? _initialUsername;
    private string? _initialPassword;
    private string? _initialWebsite;
    private string? _initialCardNumber;
    private string? _initialCardholderName;
    private string? _initialExpirationDate;
    private string? _initialCvv;
    private List<CredentialField> _initialAdditionalFields = [];
    private List<CredentialSection> _initialSections = [];

    private string? TitleErrorMessage => _isSaveAttempted && string.IsNullOrWhiteSpace(Title)
        ? "Title is required."
        : null;

    protected override void OnInitialized()
    {
        _isEditing = !IsEdit || StartInEditMode;

        _additionalFields = AdditionalFields.Select(CloneField).ToList();
        _sections = Sections.Select(CloneSection).ToList();

        // Capture snapshot
        _initialTitle = Title;
        _initialNotes = Notes;
        _initialUsername = Username;
        _initialPassword = Password;
        _initialWebsite = Website;
        _initialCardNumber = CardNumber;
        _initialCardholderName = CardholderName;
        _initialExpirationDate = ExpirationDate;
        _initialCvv = Cvv;
        _initialAdditionalFields = AdditionalFields.Select(CloneField).ToList();
        _initialSections = Sections.Select(CloneSection).ToList();
    }

    private static CredentialField CloneField(CredentialField field) => new()
    {
        Type = field.Type,
        Label = field.Label,
        Value = field.Value
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
        if (!IsEdit)
        {
            Cancel();
            return;
        }

        // Revert to initial snapshot
        Title = _initialTitle;
        Notes = _initialNotes;
        Username = _initialUsername;
        Password = _initialPassword;
        Website = _initialWebsite;
        CardNumber = _initialCardNumber;
        CardholderName = _initialCardholderName;
        ExpirationDate = _initialExpirationDate;
        Cvv = _initialCvv;
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

    private void OnTypeChanged(CredentialType? newType)
    {
        if (newType.HasValue)
        {
            Type = newType.Value;
        }
    }

    private static string GetCredentialTypeIcon(CredentialType type) => type switch
    {
        CredentialType.Login => "fa-solid fa-globe",
        CredentialType.Password => "fa-solid fa-key",
        CredentialType.Card => "fa-solid fa-credit-card",
        CredentialType.SecureNote => "fa-solid fa-note-sticky",
        _ => "fa-solid fa-shield-halved"
    };

    private static string GetCredentialTypeLabel(CredentialType type) => type switch
    {
        CredentialType.Login => "Login",
        CredentialType.Password => "Password",
        CredentialType.Card => "Card",
        CredentialType.SecureNote => "Secure Note",
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

    private void Cancel()
    {
        ModalService.Close(ModalInstance, AppModalResult.Cancel());
    }

    private void Save()
    {
        _isSaveAttempted = true;
        if (string.IsNullOrWhiteSpace(Title)) return;

        var result = new CredentialModalResult
        {
            Type = Type,
            Title = Title,
            Notes = Notes,
            AdditionalFields = _additionalFields,
            Sections = _sections,
            Username = Username,
            Password = Password,
            Website = Website,
            CardNumber = CardNumber,
            CardholderName = CardholderName,
            ExpirationDate = ExpirationDate,
            Cvv = Cvv
        };

        ModalService.Close(ModalInstance, AppModalResult.Ok(result));
    }
}

public class CredentialModalResult
{
    public CredentialType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public List<CredentialField> AdditionalFields { get; set; } = [];
    public List<CredentialSection> Sections { get; set; } = [];
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string? Website { get; set; }
    public string? CardNumber { get; set; }
    public string? CardholderName { get; set; }
    public string? ExpirationDate { get; set; }
    public string? Cvv { get; set; }
}
