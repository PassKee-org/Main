namespace PassKee.Api.Shared.Models.Vaults.Payloads;

public class LoginCredentialPayload : BaseCredentialPayload
{
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string? Website { get; set; }
}
