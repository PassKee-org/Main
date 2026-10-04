using PassKee.Api.Shared.Models.Vaults.Payloads;
using PassKee.Web.Models.Vaults;

namespace PassKee.Web.Services.Import;

public sealed record VaultImportRequest(
    Stream DatabaseStream,
    string Password,
    Guid VaultId,
    Guid? ParentDirectoryId,
    byte[] VaultKey,
    IReadOnlyList<DecryptedTag> ExistingTags);

public sealed record ImportAttachment(string Name, byte[] Data);
public sealed record ImportEntry(LoginCredentialPayload Payload, IReadOnlyList<string> Tags, IReadOnlyList<ImportAttachment> Attachments);
public sealed record ImportGroup(string Name, IReadOnlyList<ImportEntry> Entries, IReadOnlyList<ImportGroup> Groups)
{
    public IEnumerable<ImportEntry> AllEntries => Entries.Concat(Groups.SelectMany(group => group.AllEntries));
    public int WorkCount => 1 + Entries.Sum(entry => 1 + entry.Attachments.Count) + Groups.Sum(group => group.WorkCount);
}

public sealed record ImportProgress(int Completed, int Total, string Phase)
{
    public int Percent => Total == 0 ? 0 : Math.Clamp((int)(100L * Completed / Total), 0, 100);
}

public sealed record ImportIssue(string Item, string Message);

public sealed record VaultImportResult
{
    public int Directories { get; init; }
    public int Entries { get; init; }
    public int PartialEntries { get; init; }
    public int SkippedEntries { get; init; }
    public int Files { get; init; }
    public int Tags { get; init; }
    public bool Cancelled { get; init; }
    public bool HasWrites { get; init; }
    public IReadOnlyList<ImportIssue> Issues { get; init; } = [];
}

public interface IKdbxImportReader
{
    Task<ImportGroup> ReadAsync(Stream stream, string password, CancellationToken cancellationToken = default);
}

public interface IVaultImportService
{
    Task<VaultImportResult> ImportAsync(VaultImportRequest request, Action<ImportProgress>? progress = null, CancellationToken cancellationToken = default);
}