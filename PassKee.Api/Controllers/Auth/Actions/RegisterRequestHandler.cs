using System.Threading.Tasks;
using Api.Requests.Abstractions;
using AutoMapper;
using PassKee.Api.Shared.Dto.RequestsAndResponses.Auth;
using PassKee.Business.Common.Utils;
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
        var kdfParams = request.KdfParams != null
            ? new KdfParameters(request.KdfParams.Iterations, request.KdfParams.MemorySize, request.KdfParams.Parallelism)
            : null;

        var authResult = await _authService.RegisterAsync(
            request.Email,
            request.AuthHash,
            request.AuthSalt,
            request.UserPublicKey,
            request.EncryptedUserPrivateKey,
            request.EncryptedUserVaultKey,
            kdfParams
        );

        var response = _mapper.Map<AuthResponse>(authResult.User);
        response.AccessToken = authResult.JwtToken;
        return response;
    }
}
