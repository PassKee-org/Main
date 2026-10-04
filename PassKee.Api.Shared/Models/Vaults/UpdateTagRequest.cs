using System;
using Api.Requests.Abstractions;

namespace PassKee.Api.Shared.Models.Vaults;

public class UpdateTagRequest : IRequest<TagResponse>
{
    public Guid TagId { get; set; }
    public byte[] EncryptedName { get; set; } = null!;
}
