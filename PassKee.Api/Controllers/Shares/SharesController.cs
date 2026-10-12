using System.Threading.Tasks;
using Autofac;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PassKee.Api.Shared.Models.Shares;
using PassKee.Business.Mvc.Controllers;
using AspNetCore.ApiControllers.Extensions;

namespace PassKee.Api.Controllers.Shares;

[Route("api/shares")]
public class SharesController : MainApiControllerBase
{
    public SharesController(ILifetimeScope scope) : base(scope)
    {
    }

    [Authorize]
    [HttpPost]
    public Task<IActionResult> Create([FromBody] CreateCredentialShareRequest request)
        => this.RequestAsync()
            .For<CreateCredentialShareResponse>()
            .With(request);

    [AllowAnonymous]
    [HttpGet("{token}")]
    public Task<IActionResult> Get([FromRoute] string token)
        => this.RequestAsync()
            .For<CredentialShareResponse>()
            .With(new GetCredentialShareRequest { Token = token });
}
