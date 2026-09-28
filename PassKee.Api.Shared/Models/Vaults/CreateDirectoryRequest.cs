using System;
using Api.Requests.Abstractions;

namespace PassKee.Api.Shared.Models.Vaults;

public class CreateDirectoryRequest : IRequest<DirectoryResponse>
{
    public Guid VaultId { get; set; }
    public Guid? ParentDirectoryId { get; set; }
    public byte[] EncryptedName { get; set; } = null!;
}
