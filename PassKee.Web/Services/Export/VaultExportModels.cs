using PassKee.Web.Models.Vaults;

namespace PassKee.Web.Services.Export;

public sealed record VaultExportRequest(
    Guid VaultId,
    string VaultName,
    string Password,
    byte[] VaultKey,
    IReadOnlyList<DecryptedDirectory> Directories,
    IReadOnlyList<DecryptedCredential> Credentials,
    IReadOnlyList<DecryptedTag> Tags,
    IReadOnlyList<DecryptedCredential>? ArchivedCredentials = null);

public sealed record ExportField(string Label, string Value, bool IsProtected);

public sealed record ExportAttachment(string Name, byte[] Data);

public sealed record ExportEntry(
    string Title,
    string Username,
    string Password,
    string Url,
    string Notes,
    IReadOnlyList<string> Tags,
    IReadOnlyList<ExportField> CustomFields,
    IReadOnlyList<ExportAttachment> Attachments);

public sealed record ExportGroup(
    string Name,
    IReadOnlyList<ExportEntry> Entries,
    IReadOnlyList<ExportGroup> Groups)
{
    public IEnumerable<ExportEntry> AllEntries => Entries.Concat(Groups.SelectMany(group => group.AllEntries));
}

public sealed record ExportProgress(int Completed, int Total, string Phase)
{
    public int Percent => Total == 0 ? 0 : Math.Clamp((int)(100L * Completed / Total), 0, 100);
}

public sealed record ExportIssue(string Item, string Message);

public sealed record VaultExportResult
{
    public string FileName { get; init; } = string.Empty;
    public byte[] FileBytes { get; init; } = [];
    public int Directories { get; init; }
    public int Entries { get; init; }
    public int ArchivedEntries { get; init; }
    public int PartialEntries { get; init; }
    public int Files { get; init; }
    public bool Cancelled { get; init; }
    public IReadOnlyList<ExportIssue> Issues { get; init; } = [];
}

public interface IKdbxExportWriter
{
    Task<byte[]> WriteAsync(ExportGroup rootGroup, string password, CancellationToken cancellationToken = default);
}

public interface IVaultExportService
{
    Task<VaultExportResult> ExportAsync(VaultExportRequest request, Action<ExportProgress>? progress = null, CancellationToken cancellationToken = default);
}

