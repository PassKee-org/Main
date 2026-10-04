using PassKee.Web.Services.Import;

namespace PassKee.Web.Store.Import;

public sealed record StartImportAction(Guid SessionId);
public sealed record ImportProgressAction(Guid SessionId, ImportProgress Progress);
public sealed record ImportFinishedAction(Guid SessionId, VaultImportResult? Result, string? Error = null);
public sealed record ClearImportAction(Guid SessionId);