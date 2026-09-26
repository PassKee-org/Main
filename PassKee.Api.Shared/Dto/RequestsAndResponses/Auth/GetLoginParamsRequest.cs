using System.ComponentModel.DataAnnotations;
using Api.Requests.Abstractions;

namespace PassKee.Api.Shared.Dto.RequestsAndResponses.Auth;

public class GetLoginParamsRequest : IRequest<LoginParamsResponse>
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;
}
