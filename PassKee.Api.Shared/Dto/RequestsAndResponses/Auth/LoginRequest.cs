using System.ComponentModel.DataAnnotations;
using Api.Requests.Abstractions;

namespace PassKee.Api.Shared.Dto.RequestsAndResponses.Auth;

public class LoginRequest : IRequest<AuthResponse>
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;

    [Required]
    public byte[] AuthHash { get; set; } = null!;
}
