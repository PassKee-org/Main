namespace PassKee.Api.Shared.Models.Vaults.Payloads;

public class ServerCredentialPayload : BaseCredentialPayload
{
    public string? Url { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
}
