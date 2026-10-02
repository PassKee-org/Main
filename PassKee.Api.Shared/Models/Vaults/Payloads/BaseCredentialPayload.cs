namespace PassKee.Api.Shared.Models.Vaults.Payloads;

public abstract class BaseCredentialPayload
{
    public string Title { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public List<CredentialField> AdditionalFields { get; set; } = [];
    public List<CredentialSection> Sections { get; set; } = [];
}
