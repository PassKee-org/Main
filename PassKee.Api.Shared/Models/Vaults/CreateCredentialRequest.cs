using System;
using Api.Requests.Abstractions;
using PassKee.Business.Common.Constants;

namespace PassKee.Api.Shared.Models.Vaults;

public class CreateCredentialRequest : IRequest<CredentialResponse>
{
    public Guid VaultId { get; set; }
    public Guid? DirectoryId { get; set; }
    public CredentialType Type { get; set; }
    public byte[] EncryptedBody { get; set; } = null!;
}
