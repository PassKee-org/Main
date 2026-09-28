using System;
using Api.Requests.Abstractions;

namespace PassKee.Api.Shared.Models.Vaults;

public class DeleteDirectoryRequest : IRequest<ActionResponse>
{
    public Guid DirectoryId { get; set; }
}
