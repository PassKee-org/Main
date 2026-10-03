using System;
using Api.Requests.Abstractions;
using AspNetCore.ApiControllers.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace PassKee.Api.Models.Storage.Requests;

public class GetFileRequest : IRequest<FileResponse>
{
    [FromRoute(Name = "fileId")]
    public Guid FileId { get; set; }
}
