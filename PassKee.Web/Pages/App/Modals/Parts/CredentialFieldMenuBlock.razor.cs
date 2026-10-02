using System;
using Microsoft.AspNetCore.Components;
using PassKee.Api.Shared.Models.Vaults.Payloads;

namespace PassKee.Web.Pages.App.Modals.Parts;

public partial class CredentialFieldMenuBlock : ComponentBase
{
    [Parameter] public string Label { get; set; } = "Add more";
    [Parameter] public bool AllowSections { get; set; }
    [Parameter] public EventCallback<CredentialFieldType> OnAddField { get; set; }
    [Parameter] public EventCallback OnAddSection { get; set; }

    public static string GetFieldLabel(CredentialFieldType type) => type switch
    {
        CredentialFieldType.Url => "URL",
        CredentialFieldType.Email => "Email",
        CredentialFieldType.OneTimePassword => "One-time password",
        CredentialFieldType.SecurityQuestion => "Security question",
        _ => type.ToString()
    };

    public static string GetFieldIcon(CredentialFieldType type) => type switch
    {
        CredentialFieldType.Url => "fa-solid fa-link",
        CredentialFieldType.Email => "fa-solid fa-envelope",
        CredentialFieldType.Address => "fa-solid fa-location-dot",
        CredentialFieldType.Date => "fa-solid fa-calendar",
        CredentialFieldType.OneTimePassword => "fa-solid fa-clock",
        CredentialFieldType.Password => "fa-solid fa-key",
        CredentialFieldType.Phone => "fa-solid fa-phone",
        CredentialFieldType.SecurityQuestion => "fa-solid fa-circle-question",
        _ => "fa-solid fa-font"
    };
}