using Fluxor;
using PassKee.Web.Services.Export;

namespace PassKee.Web.Store.Export;

[FeatureState]
public sealed record ExportState
{
    public Guid SessionId { get; init; }
    public bool IsRunning { get; init; }
    public ExportProgress Progress { get; init; } = new(0, 0, "Preparing backup");
    public VaultExportResult? Result { get; init; }
    public string? Error { get; init; }
}

