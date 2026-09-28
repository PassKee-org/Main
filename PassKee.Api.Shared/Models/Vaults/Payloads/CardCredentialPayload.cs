namespace PassKee.Api.Shared.Models.Vaults.Payloads;

public class CardCredentialPayload : BaseCredentialPayload
{
    public string? CardNumber { get; set; }
    public string? CardholderName { get; set; }
    public string? ExpirationDate { get; set; }
    public string? Cvv { get; set; }
    public string? Pin { get; set; }
}
