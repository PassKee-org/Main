using System;
using Api.Requests.Abstractions;

namespace PassKee.Api.Shared.Models.Vaults;

public class DeleteCredentialResponse : IResponse
{
    public Guid CredentialId { get; set; }
}
