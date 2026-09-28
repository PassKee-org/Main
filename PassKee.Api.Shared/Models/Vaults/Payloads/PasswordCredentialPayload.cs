namespace PassKee.Api.Shared.Models.Vaults.Payloads;

public class PasswordCredentialPayload : BaseCredentialPayload
{
    public string? Username { get; set; }
    public string? Password { get; set; }
}
