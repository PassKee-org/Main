using System;
using System.Collections.Generic;
using System.Linq;
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
    [Parameter] public CredentialType Type { get; set; } = CredentialType.Login;
    [Parameter] public string Title { get; set; } = string.Empty;
    [Parameter] public string? Notes { get; set; }
    [Parameter] public List<CredentialField> AdditionalFields { get; set; } = [];
    [Parameter] public List<CredentialSection> Sections { get; set; } = [];

    private List<CredentialField> _additionalFields = [];
    private List<CredentialSection> _sections = [];
    private bool _isSaveAttempted;

    private string? TitleErrorMessage => _isSaveAttempted && string.IsNullOrWhiteSpace(Title)
        ? "Title is required."
        : null;

    protected override void OnInitialized()
    {
        _additionalFields = AdditionalFields.Select(CloneField).ToList();
        _sections = Sections.Select(section => new CredentialSection
        {
            Title = section.Title,
            Fields = section.Fields.Select(CloneField).ToList()
        }).ToList();
    }

    private static CredentialField CloneField(CredentialField field) => new()
    {
        Type = field.Type,
        Label = field.Label,
        Value = field.Value
    };

    private void AddField(CredentialFieldType type)
    {
        _additionalFields.Add(new CredentialField { Type = type, Label = Parts.CredentialFieldMenuBlock.GetFieldLabel(type) });
    }

    private void AddSection()
    {
        _sections.Add(new CredentialSection { Title = "New section" });
    }
    
    // Login/Password
    [Parameter] public string? Username { get; set; }
    [Parameter] public string? Password { get; set; }
    [Parameter] public string? Website { get; set; }

    // Card
    [Parameter] public string? CardNumber { get; set; }
    [Parameter] public string? CardholderName { get; set; }
    [Parameter] public string? ExpirationDate { get; set; }
    [Parameter] public string? Cvv { get; set; }

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
