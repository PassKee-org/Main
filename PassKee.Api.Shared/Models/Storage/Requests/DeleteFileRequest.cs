using System;
using Api.Requests.Abstractions;
using PassKee.Api.Shared.Models.Vaults;

namespace PassKee.Api.Shared.Models.Storage.Requests;

public class DeleteFileRequest : IRequest<ActionResponse>
{
    public Guid FileId { get; set; }
}
