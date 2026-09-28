using System;

namespace PassKee.Api.Shared.Models.Vaults;

public class DirectoryDto
{
    public Guid Id { get; set; }
    public Guid VaultId { get; set; }
    public Guid? ParentDirectoryId { get; set; }
    public byte[] EncryptedName { get; set; } = null!;
}
