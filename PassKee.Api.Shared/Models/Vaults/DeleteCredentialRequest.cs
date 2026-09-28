using System;
using Api.Requests.Abstractions;

namespace PassKee.Api.Shared.Models.Vaults;

public class DeleteCredentialRequest : IRequest<ActionResponse>
{
    public Guid CredentialId { get; set; }
}
