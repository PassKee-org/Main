using System.Collections.Generic;
using Api.Requests.Abstractions;

namespace PassKee.Api.Shared.Models.Vaults;

public class ArchivedCredentialsResponse : IResponse
{
    public List<CredentialDto> Credentials { get; set; } = [];
}

