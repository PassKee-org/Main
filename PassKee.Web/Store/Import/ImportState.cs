using Fluxor;
using PassKee.Web.Services.Import;

namespace PassKee.Web.Store.Import;

[FeatureState]
public sealed record ImportState
{
    public Guid SessionId { get; init; }
    public bool IsRunning { get; init; }
    public ImportProgress Progress { get; init; } = new(0, 0, "Opening database");
    public VaultImportResult? Result { get; init; }
    public string? Error { get; init; }
}