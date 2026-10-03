using System;
using System.Threading.Tasks;
using Autofac;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PassKee.Api.Shared.Models.Storage.Requests;
using PassKee.Api.Shared.Models.Storage;
using PassKee.Api.Shared.Models.Vaults;
using AspNetCore.ApiControllers.Abstractions;
using AspNetCore.ApiControllers.Extensions;
using PassKee.Api.Models.Storage.Requests;
using PassKee.Business.Common.Constants;
using PassKee.Business.Mvc.Controllers;

namespace PassKee.Api.Controllers.Storage;

[ApiController]
[Route("api/storage")]
[Authorize]
public class StorageController : MainApiControllerBase
{
    public StorageController(ILifetimeScope scope) : base(scope) { }

    [HttpPost("upload")]
    [RequestSizeLimit(FileStorageConstants.MaxUploadRequestSize)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public Task<IActionResult> Upload([FromForm] UploadRequest request)
        => this.RequestAsync()
            .For<StoredFileDto>()
            .With(request);

    [HttpGet("file/{fileId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public Task<IActionResult> GetFile([FromRoute] Guid fileId)
        => this.RequestAsync()
            .For<FileResponse>()
            .With(new GetFileRequest { FileId = fileId });

    [HttpDelete("file/{fileId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public Task<IActionResult> DeleteFile([FromRoute] Guid fileId)
        => this.RequestAsync()
            .For<ActionResponse>()
            .With(new DeleteFileRequest { FileId = fileId });
}
