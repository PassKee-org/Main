using System;
using PassKee.Business.Common.Constants;

namespace PassKee.Api.Shared.Models.Vaults;

public class CredentialDto
{
    public Guid Id { get; set; }
    public Guid VaultId { get; set; }
    public Guid? DirectoryId { get; set; }
    public CredentialType Type { get; set; }
    public byte[] EncryptedBody { get; set; } = null!;
    public DateTime? ArchivedAt { get; set; }
}
