using System;
using Api.Requests.Abstractions;

namespace PassKee.Api.Shared.Models.Vaults;

public class CreateTagRequest : IRequest<TagResponse>
{
    public Guid VaultId { get; set; }
    public byte[] EncryptedName { get; set; } = null!;
}
