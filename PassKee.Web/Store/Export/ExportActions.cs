using PassKee.Web.Services.Export;

namespace PassKee.Web.Store.Export;

public sealed record StartExportAction(Guid SessionId);
public sealed record ExportProgressAction(Guid SessionId, ExportProgress Progress);
public sealed record ExportFinishedAction(Guid SessionId, VaultExportResult? Result, string? Error = null);
public sealed record ClearExportAction(Guid SessionId);

