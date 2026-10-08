using System;
using System.Collections.Generic;
using Api.Requests.Abstractions;

namespace PassKee.Api.Shared.Models.Vaults;

public class DeleteDirectoryResponse : ActionResponse
{
    public Guid DirectoryId { get; set; }
    public List<Guid> DeletedDirectoryIds { get; set; } = [];
    public List<Guid> DeletedCredentialIds { get; set; } = [];
}

