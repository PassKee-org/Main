using System.Threading.Tasks;
using Api.Requests.Abstractions;
using AutoMapper;
using PassKee.Api.Shared.Dto.RequestsAndResponses.Auth;
using PassKee.Business.Services.Auth;

namespace PassKee.Api.Controllers.Auth.Actions;

public class LoginRequestHandler : IAsyncRequestHandler<LoginRequest, AuthResponse>
{
    private readonly IAuthService _authService;
    private readonly IMapper _mapper;

    public LoginRequestHandler(IAuthService authService, IMapper mapper)
    {
        _authService = authService;
        _mapper = mapper;
    }

    public async Task<AuthResponse> ExecuteAsync(LoginRequest request)
    {
        var authResult = await _authService.LoginAsync(
            request.Email,
            request.AuthHash
        );

        var response = _mapper.Map<AuthResponse>(authResult.User);
        response.AccessToken = authResult.JwtToken;
        return response;
    }
}
