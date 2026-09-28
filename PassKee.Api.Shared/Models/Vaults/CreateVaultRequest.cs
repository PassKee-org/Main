using Api.Requests.Abstractions;

namespace PassKee.Api.Shared.Models.Vaults;

public class CreateVaultRequest : IRequest<VaultResponse>
{
    public string Name { get; set; } = string.Empty;
    public byte[]? EncryptedVaultKey { get; set; }
}
