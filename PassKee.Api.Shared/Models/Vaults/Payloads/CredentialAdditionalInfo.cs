using PassKee.Api.Shared.Models.Storage;

namespace PassKee.Api.Shared.Models.Vaults.Payloads;

public enum CredentialFieldType
{
    Text,
    Url,
    Email,
    Address,
    Date,
    OneTimePassword,
    Password,
    Phone,
    SecurityQuestion,
    File
}

public class CredentialField
{
    public CredentialFieldType Type { get; set; }
    public string Label { get; set; } = string.Empty;
    public string? Value { get; set; }

    /// <summary>
    /// Uploaded file reference, used when <see cref="Type"/> is <see cref="CredentialFieldType.File"/>.
    /// </summary>
    public StoredFileDto? File { get; set; }
}

public class CredentialSection
{
    public string Title { get; set; } = string.Empty;
    public List<CredentialField> Fields { get; set; } = [];
}