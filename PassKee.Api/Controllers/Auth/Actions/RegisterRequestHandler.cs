using System.Threading.Tasks;
using Api.Requests.Abstractions;
using AutoMapper;
using PassKee.Api.Shared.Dto.RequestsAndResponses.Auth;
using PassKee.Business.Services.Auth;

namespace PassKee.Api.Controllers.Auth.Actions;

public class RegisterRequestHandler : IAsyncRequestHandler<RegisterRequest, AuthResponse>
{
    private readonly IAuthService _authService;
    private readonly IMapper _mapper;

    public RegisterRequestHandler(IAuthService authService, IMapper mapper)
    {
        _authService = authService;
        _mapper = mapper;
    }

    public async Task<AuthResponse> ExecuteAsync(RegisterRequest request)
    {
        var authResult = await _authService.RegisterAsync(
            request.Email,
            request.AuthHash,
            request.AuthSalt,
            request.UserPublicKey,
            request.EncryptedUserPrivateKey,
            request.EncryptedUserVaultKey,
            request.KdfParams
        );

        var response = _mapper.Map<AuthResponse>(authResult.User);
        response.AccessToken = authResult.JwtToken;
        return response;
    }
}
