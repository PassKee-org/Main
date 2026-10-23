using System;
using System.Collections.Generic;
using Api.Requests.Abstractions;

namespace PassKee.Api.Shared.Models.Vaults;

public class DeleteCredentialsResponse : IResponse
{
    public List<Guid> CredentialIds { get; set; } = [];
}
