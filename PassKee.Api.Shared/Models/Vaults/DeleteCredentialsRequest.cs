using System;
using System.Collections.Generic;
using Api.Requests.Abstractions;

namespace PassKee.Api.Shared.Models.Vaults;

public class DeleteCredentialsRequest : IRequest<DeleteCredentialsResponse>
{
    public List<Guid> CredentialIds { get; set; } = [];
}

