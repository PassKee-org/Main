using System.Threading.Tasks;
using Api.Requests.Abstractions;
using AutoMapper;
using PassKee.Api.Shared.Dto.RequestsAndResponses.Auth;
using PassKee.Business.Services.Auth;
using PassKee.Business.Services.Http;

namespace PassKee.Api.Controllers.Auth.Actions;

public class LoginRequestHandler : IAsyncRequestHandler<LoginRequest, AuthResponse>
{
    private readonly IAuthService _authService;
    private readonly IHttpCookiesService _cookiesService;
    private readonly IMapper _mapper;

    public LoginRequestHandler(
        IAuthService authService,
        IHttpCookiesService cookiesService,
        IMapper mapper)
    {
        _authService = authService;
        _cookiesService = cookiesService;
        _mapper = mapper;
    }

    public async Task<AuthResponse> ExecuteAsync(LoginRequest request)
    {
        var authResult = await _authService.LoginAsync(
            request.Email,
            request.AuthHash
        );

        _cookiesService.AppendAuthCookies(authResult.AccessToken, authResult.JwtToken);

        return _mapper.Map<AuthResponse>(authResult.User);
    }
}
