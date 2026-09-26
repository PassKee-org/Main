using Api.Requests.Abstractions;

namespace PassKee.Api.Shared.Dto.RequestsAndResponses.Auth;

public class AuthResponse : IResponse
{
    public string AccessToken { get; set; } = null!;
    public string? UserPublicKey { get; set; }
    public string? EncryptedUserPrivateKey { get; set; }
    public string? EncryptedUserVaultKey { get; set; }
}
