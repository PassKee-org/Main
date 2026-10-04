using System;

namespace PassKee.Api.Shared.Models.Vaults;

public class TagDto
{
    public Guid Id { get; set; }
    public Guid VaultId { get; set; }
    public byte[] EncryptedName { get; set; } = null!;
}
