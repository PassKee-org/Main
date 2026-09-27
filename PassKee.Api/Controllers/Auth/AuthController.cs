using System.Threading.Tasks;
using AspNetCore.ApiControllers.Extensions;
using Autofac;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PassKee.Api.Shared.Dto.RequestsAndResponses.Auth;
using PassKee.Business.Mvc.Controllers;

namespace PassKee.Api.Controllers.Auth;

[ApiController]
[Route("api/[controller]")]
public class AuthController : MainApiControllerBase
{
    public AuthController(ILifetimeScope scope) : base(scope)
    {
    }

    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<IActionResult> Register([FromBody] RegisterRequest request)
        => this.RequestAsync()
            .For<AuthResponse>()
            .With(request);

    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<IActionResult> Login([FromBody] LoginRequest request)
        => this.RequestAsync()
            .For<AuthResponse>()
            .With(request);

    [HttpGet("login-params")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<IActionResult> GetLoginParams([FromQuery] GetLoginParamsRequest request)
        => this.RequestAsync()
            .For<LoginParamsResponse>()
            .With(request);

    [HttpGet("check")]
    [Microsoft.AspNetCore.Authorization.Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public Task<IActionResult> Check([FromQuery] CheckAuthRequest? request = null)
        => this.RequestAsync(request ?? new CheckAuthRequest());

    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public Task<IActionResult> Logout([FromBody] LogoutRequest? request = null)
        => this.RequestAsync(request ?? new LogoutRequest());
}

