using System.Threading.Tasks;
using PassKee.Api.Shared.Dto.RequestsAndResponses.Auth;

namespace PassKee.Web.Services.Auth;

public record RegisterResult(AuthResponse Response, string SecretKey, byte[] UserPrivateKey, byte[] UserPublicKey)
{
    public string SecretKeyBase64 => SecretKey;
}
public record LoginResult(AuthResponse Response, byte[]? UserPrivateKey, byte[]? UserPublicKey);

public interface IAuthClientService
{
    Task<RegisterResult> RegisterAsync(string email, string password, string? secretKey = null);
    Task<LoginResult> LoginAsync(string email, string password, string secretKey);
    Task<LoginResult> LoginAsync(string email, string password, byte[] secretKeyBytes);
    Task<LoginResult> UnlockWithMasterPasswordAsync(string password);
}

