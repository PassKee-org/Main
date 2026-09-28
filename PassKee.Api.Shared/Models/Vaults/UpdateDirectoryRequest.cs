using System;
using Api.Requests.Abstractions;

namespace PassKee.Api.Shared.Models.Vaults;

public class UpdateDirectoryRequest : IRequest<DirectoryResponse>
{
    public Guid DirectoryId { get; set; }
    public Guid? ParentDirectoryId { get; set; }
    public byte[] EncryptedName { get; set; } = null!;
}
