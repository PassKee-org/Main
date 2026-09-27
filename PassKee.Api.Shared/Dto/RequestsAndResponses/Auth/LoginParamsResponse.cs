using Api.Requests.Abstractions;

namespace PassKee.Api.Shared.Dto.RequestsAndResponses.Auth;

public class LoginParamsResponse : IResponse
{
    public string AuthSalt { get; set; } = null!;
    public KdfParamsDto? KdfParams { get; set; }
}
