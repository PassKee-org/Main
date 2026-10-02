using System.Collections.Generic;
using Microsoft.AspNetCore.Components;
using PassKee.Api.Shared.Models.Vaults.Payloads;

namespace PassKee.Web.Shared.Modals.Parts;

public partial class CredentialFieldsBlock : ComponentBase
{
    [Parameter] public List<CredentialField> Fields { get; set; } = [];

    private static string GetInputType(CredentialFieldType type) => type switch
    {
        CredentialFieldType.Url => "url",
        CredentialFieldType.Email => "email",
        CredentialFieldType.Date => "date",
        CredentialFieldType.Phone => "tel",
        CredentialFieldType.Password or CredentialFieldType.OneTimePassword or CredentialFieldType.SecurityQuestion => "password",
        _ => "text"
    };
}
