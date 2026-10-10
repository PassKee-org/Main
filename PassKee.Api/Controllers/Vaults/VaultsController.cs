using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Autofac;
using PassKee.Api.Shared.Models.Vaults;
using PassKee.Business.Mvc.Controllers;
using AspNetCore.ApiControllers.Extensions;

namespace PassKee.Api.Controllers.Vaults;

[Route("api/vaults")]
[Authorize]
public class VaultsController : MainApiControllerBase
{
    public VaultsController(ILifetimeScope scope) : base(scope)
    {
    }

    [HttpGet]
    public Task<IActionResult> GetVaults()
        => this.RequestAsync()
            .For<VaultsResponse>()
            .With(new GetVaultsRequest());

    [HttpGet("{vaultId:guid}")]
    public Task<IActionResult> GetVaultDetails([FromRoute] System.Guid vaultId)
        => this.RequestAsync()
            .For<VaultDetailsResponse>()
            .With(new GetVaultDetailsRequest { VaultId = vaultId });

    [HttpGet("{vaultId:guid}/credentials/archived")]
    public Task<IActionResult> GetArchivedCredentials([FromRoute] System.Guid vaultId)
        => this.RequestAsync()
            .For<ArchivedCredentialsResponse>()
            .With(new GetArchivedCredentialsRequest { VaultId = vaultId });

    [HttpPost]
    public Task<IActionResult> CreateVault([FromBody] CreateVaultRequest request)
        => this.RequestAsync()
            .For<VaultResponse>()
            .With(request);
}
