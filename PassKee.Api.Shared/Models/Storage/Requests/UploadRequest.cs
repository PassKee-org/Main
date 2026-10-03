using System;
using Api.Requests.Abstractions;
using Microsoft.AspNetCore.Http;

namespace PassKee.Api.Shared.Models.Storage.Requests;

public class UploadRequest : IRequest<StoredFileDto>
{
    public Guid EntityId { get; set; }
    public StorageEntityType EntityType { get; set; }
    public IFormFile File { get; set; } = null!;
}

public enum StorageEntityType
{
    Vault = 1
}
