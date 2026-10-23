using System;
using Api.Requests.Abstractions;

namespace PassKee.Api.Shared.Models.Storage.Requests;

public class DeleteFileRequest : IRequest
{
    public Guid FileId { get; set; }
}
