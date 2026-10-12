using Api.Requests.Abstractions;

namespace PassKee.Api.Shared.Models.Shares;

public class GetCredentialShareRequest : IRequest<CredentialShareResponse>
{
    public string Token { get; set; } = string.Empty;
}

