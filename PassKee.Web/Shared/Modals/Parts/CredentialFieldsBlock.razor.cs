using System.Collections.Generic;
using Microsoft.AspNetCore.Components;
using PassKee.Api.Shared.Models.Vaults.Payloads;

namespace PassKee.Web.Shared.Modals.Parts;

public partial class CredentialFieldsBlock : ComponentBase
{
    [Parameter] public List<CredentialField> Fields { get; set; } = [];
}
