using System;
using Api.Requests.Abstractions;

namespace PassKee.Api.Shared.Models.Vaults;

public class DeleteCredentialResponse : ActionResponse
{
    public Guid CredentialId { get; set; }
}

