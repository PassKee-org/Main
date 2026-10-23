using PassKee.Business.Common.Constants;

namespace PassKee.Api.Shared.Models.Vaults.Payloads;

public abstract class BaseCredentialPayload
{
    /// <summary>
    /// Type marker encrypted inside the ciphertext payload to prevent ciphertext transplanting / type confusion attacks.
    /// </summary>
    public CredentialType? PayloadType { get; set; }

    public string Title { get; set; } = string.Empty;
    public string? Notes { get; set; }
    /// <summary>
    /// Custom icon (Emoji or HTML symbol) selected by the user. If null, default type icon is used.
    /// </summary>
    public string? Icon { get; set; }
    public List<CredentialField> AdditionalFields { get; set; } = [];
    public List<CredentialSection> Sections { get; set; } = [];
    public List<Guid> TagIds { get; set; } = [];
}
