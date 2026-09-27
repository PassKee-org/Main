using System.ComponentModel.DataAnnotations;
using Api.Requests.Abstractions;

namespace PassKee.Api.Shared.Dto.RequestsAndResponses.Auth;

public class RegisterRequest : IRequest<AuthResponse>
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;

    [Required]
    public byte[] AuthHash { get; set; } = null!;

    [Required]
    public byte[] AuthSalt { get; set; } = null!;

    public KdfParamsRequest? KdfParams { get; set; }

    [Required]
    public byte[] UserPublicKey { get; set; } = null!;

    [Required]
    public byte[] EncryptedUserPrivateKey { get; set; } = null!;

    [Required]
    public byte[] EncryptedUserVaultKey { get; set; } = null!;
}
