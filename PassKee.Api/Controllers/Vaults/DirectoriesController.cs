using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Autofac;
using PassKee.Api.Shared.Models.Vaults;
using PassKee.Business.Mvc.Controllers;
using AspNetCore.ApiControllers.Extensions;

namespace PassKee.Api.Controllers.Vaults;

[Route("api/vaults/directories")]
[Authorize]
public class DirectoriesController : MainApiControllerBase
{
    public DirectoriesController(ILifetimeScope scope) : base(scope)
    {
    }

    [HttpPost]
    public Task<IActionResult> Create([FromBody] CreateDirectoryRequest request)
        => this.RequestAsync()
            .For<DirectoryResponse>()
            .With(request);

    [HttpPut("{directoryId:guid}")]
    public Task<IActionResult> Update([FromRoute] System.Guid directoryId, [FromBody] UpdateDirectoryRequest request)
    {
        request.DirectoryId = directoryId;
        return this.RequestAsync()
            .For<DirectoryResponse>()
            .With(request);
    }

    [HttpDelete("{directoryId:guid}")]
    public Task<IActionResult> Delete([FromRoute] System.Guid directoryId)
        => this.RequestAsync()
            .For<DeleteDirectoryResponse>()
            .With(new DeleteDirectoryRequest { DirectoryId = directoryId });
}
