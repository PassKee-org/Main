using System;
using Api.Requests.Abstractions;

namespace PassKee.Api.Shared.Models.Vaults;

public class DeleteDirectoryRequest : IRequest<DeleteDirectoryResponse>
{
    public Guid DirectoryId { get; set; }
}
