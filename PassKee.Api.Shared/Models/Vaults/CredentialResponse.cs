using Api.Requests.Abstractions;

namespace PassKee.Api.Shared.Models.Vaults;

public class CredentialResponse : IResponse
{
    public CredentialDto Credential { get; set; } = null!;
}
