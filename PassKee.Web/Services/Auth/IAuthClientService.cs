using System.Threading.Tasks;
using PassKee.Api.Shared.Dto.RequestsAndResponses.Auth;

namespace PassKee.Web.Services.Auth;

public record RegisterResult(AuthResponse Response, string SecretKeyBase64, byte[] UserPrivateKey, byte[] UserPublicKey);
public record LoginResult(AuthResponse Response, byte[]? UserPrivateKey, byte[]? UserPublicKey);

public interface IAuthClientService
{
    Task<RegisterResult> RegisterAsync(string email, string password);
    Task<LoginResult> LoginAsync(string email, string password, string secretKeyBase64);
    Task<LoginResult> UnlockWithMasterPasswordAsync(string password);
}

