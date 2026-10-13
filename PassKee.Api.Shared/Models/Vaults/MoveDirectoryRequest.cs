using System;
using Api.Requests.Abstractions;

namespace PassKee.Api.Shared.Models.Vaults;

public class MoveDirectoryRequest : IRequest<DirectoryResponse>
{
    public Guid DirectoryId { get; set; }
    public Guid? ParentDirectoryId { get; set; }
}

