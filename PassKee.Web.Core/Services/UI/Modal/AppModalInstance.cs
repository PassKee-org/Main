using Microsoft.AspNetCore.Components;

namespace PassKee.Web.Core.Services.UI.Modal;

public class AppModalInstance
{
    public Guid Id { get; } = Guid.NewGuid();

    public Type ComponentType { get; init; } = default!;

    public IDictionary<string, object?>? Parameters { get; init; }

    public AppModalOptions Options { get; init; } = new();

    public Action<AppModalResult>? OnClosedCallback { get; init; }

    internal TaskCompletionSource<AppModalResult> Tcs { get; } = new();

    private readonly IAppModalDialogService _service;

    public AppModalInstance(IAppModalDialogService service)
    {
        _service = service;
    }

    public Task Close(AppModalResult? result = null)
    {
        _service.Close(this, result);
        return Task.CompletedTask;
    }

    public Task CloseAsync(AppModalResult? result = null)
    {
        _service.Close(this, result);
        return Task.CompletedTask;
    }
}
