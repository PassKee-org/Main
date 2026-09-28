using Api.Requests.Abstractions;

namespace PassKee.Api.Shared.Models.Vaults;

public class VaultResponse : IResponse
{
    public VaultDto Vault { get; set; } = null!;
}
