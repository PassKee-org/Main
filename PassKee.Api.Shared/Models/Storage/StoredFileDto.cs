using System;
using Api.Requests.Abstractions;

namespace PassKee.Api.Shared.Models.Storage;

public class StoredFileDto : IResponse
{
    public Guid Id { get; set; }
    public Guid? VaultId { get; set; }
    public long Size { get; set; }

    /// <summary>
    /// Original file name. Never populated by the server (zero-knowledge): the client sets it after upload
    /// and it is persisted only inside the encrypted credential payload.
    /// </summary>
    public string? FileName { get; set; }
}
