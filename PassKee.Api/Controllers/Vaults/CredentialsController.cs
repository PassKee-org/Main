using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Autofac;
using PassKee.Api.Shared.Models.Vaults;
using PassKee.Business.Mvc.Controllers;
using AspNetCore.ApiControllers.Extensions;

namespace PassKee.Api.Controllers.Vaults;

[Route("api/vaults/credentials")]
[Authorize]
public class CredentialsController : MainApiControllerBase
{
    public CredentialsController(ILifetimeScope scope) : base(scope)
    {
    }

    [HttpPost]
    public Task<IActionResult> Create([FromBody] CreateCredentialRequest request)
        => this.RequestAsync()
            .For<CredentialResponse>()
            .With(request);

    [HttpPut("{credentialId:guid}")]
    public Task<IActionResult> Update([FromRoute] System.Guid credentialId, [FromBody] UpdateCredentialRequest request)
    {
        request.CredentialId = credentialId;
        return this.RequestAsync()
            .For<CredentialResponse>()
            .With(request);
    }

    [HttpDelete("{credentialId:guid}")]
    public Task<IActionResult> Delete([FromRoute] System.Guid credentialId)
        => this.RequestAsync()
            .For<DeleteCredentialResponse>()
            .With(new DeleteCredentialRequest { CredentialId = credentialId });

    [HttpPost("batch-delete")]
    public Task<IActionResult> BatchDelete([FromBody] DeleteCredentialsRequest request)
        => this.RequestAsync()
            .For<DeleteCredentialsResponse>()
            .With(request);

    [HttpPost("{credentialId:guid}/archive")]
    public Task<IActionResult> Archive([FromRoute] System.Guid credentialId)
        => this.RequestAsync()
            .For<CredentialResponse>()
            .With(new ArchiveCredentialRequest { CredentialId = credentialId });

    [HttpPost("update-positions")]
    public Task<IActionResult> UpdatePositions([FromBody] UpdateCredentialPositionsRequest request)
        => this.RequestAsync(request);
}
