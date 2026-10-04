using System;
using System.Threading.Tasks;
using AspNetCore.ApiControllers.Extensions;
using Autofac;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PassKee.Api.Shared.Models.Vaults;
using PassKee.Business.Mvc.Controllers;

namespace PassKee.Api.Controllers.Vaults;

[Route("api/vaults/tags")]
[Authorize]
public class TagsController : MainApiControllerBase
{
    public TagsController(ILifetimeScope scope) : base(scope)
    {
    }

    [HttpPost]
    public Task<IActionResult> Create([FromBody] CreateTagRequest request)
        => this.RequestAsync()
            .For<TagResponse>()
            .With(request);

    [HttpPut("{tagId:guid}")]
    public Task<IActionResult> Update([FromRoute] Guid tagId, [FromBody] UpdateTagRequest request)
    {
        request.TagId = tagId;
        return this.RequestAsync()
            .For<TagResponse>()
            .With(request);
    }
}
