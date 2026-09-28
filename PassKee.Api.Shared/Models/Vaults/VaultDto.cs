using System;

namespace PassKee.Api.Shared.Models.Vaults;

public class VaultDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public byte[]? EncryptedVaultKey { get; set; }
}
