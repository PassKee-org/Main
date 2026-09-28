using System;
using Api.Requests.Abstractions;
using PassKee.Api.Shared.Models.Vaults.Enums;

namespace PassKee.Api.Shared.Models.Vaults;

public class UpdateCredentialRequest : IRequest<CredentialResponse>
{
    public Guid CredentialId { get; set; }
    public Guid? DirectoryId { get; set; }
    public CredentialType Type { get; set; }
    public byte[] EncryptedBody { get; set; } = null!;
}
