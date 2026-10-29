using System;
using System.Threading.Tasks;
using PassKee.Web.Constants.Navigation;

namespace PassKee.Web.Services.UI.Viewport;

public interface IViewportService : IAsyncDisposable
{
    bool IsMobile { get; }
    bool IsInitialized { get; }
    AppViewportMode Mode { get; }

    event Action? OnViewportChanged;

    ValueTask InitializeAsync();
    void SetMode(AppViewportMode mode);
}

