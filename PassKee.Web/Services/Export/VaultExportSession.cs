namespace PassKee.Web.Services.Export;

public sealed class VaultExportSession : IDisposable
{
    private VaultExportRequest? _request;
    private CancellationTokenSource? _cancellation;
    private Guid _id;

    public Guid Begin(VaultExportRequest request)
    {
        if (_request != null) throw new InvalidOperationException("An export is already running.");
        _id = Guid.NewGuid();
        _request = request with { VaultKey = request.VaultKey.ToArray() };
        _cancellation = new CancellationTokenSource();
        return _id;
    }

    public (VaultExportRequest Request, CancellationToken Token)? Get(Guid id) =>
        id == _id && _request != null && _cancellation != null ? (_request, _cancellation.Token) : null;

    public void Cancel(Guid id)
    {
        if (id == _id) _cancellation?.Cancel();
    }

    public void Release(Guid id)
    {
        if (id != _id) return;
        if (_request != null)
        {
            Array.Clear(_request.VaultKey);
        }
        _request = null;
        _cancellation?.Dispose();
        _cancellation = null;
    }

    public void Dispose()
    {
        Cancel(_id);
        Release(_id);
    }
}

