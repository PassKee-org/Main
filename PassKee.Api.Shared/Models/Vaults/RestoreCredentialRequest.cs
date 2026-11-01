using System;
using Api.Requests.Abstractions;

namespace PassKee.Api.Shared.Models.Vaults;

public class RestoreCredentialRequest : IRequest<CredentialResponse>
{
    public Guid CredentialId { get; set; }
}

