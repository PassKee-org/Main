using System;
using System.Threading.Tasks;
using Microsoft.JSInterop;
using PassKee.Web.Constants.Navigation;

namespace PassKee.Web.Services.UI.Viewport;

public class BrowserViewportService(IJSRuntime jsRuntime) : IViewportService
{
    private DotNetObjectReference<BrowserViewportService>? _dotNetRef;
    private bool _isMobile;
    private bool _isDisposed;

    public bool IsInitialized { get; private set; }
    public AppViewportMode Mode { get; private set; } = AppViewportMode.Auto;

    public bool IsMobile => Mode switch
    {
        AppViewportMode.Mobile => true,
        AppViewportMode.Desktop => false,
        _ => _isMobile
    };

    public event Action? OnViewportChanged;

    public async ValueTask InitializeAsync()
    {
        if (IsInitialized)
        {
            return;
        }

        try
        {
            _dotNetRef = DotNetObjectReference.Create(this);
            _isMobile = await jsRuntime.InvokeAsync<bool>("PassKeeInterop.initViewportListener", _dotNetRef);
            IsInitialized = true;
            OnViewportChanged?.Invoke();
        }
        catch (JSDisconnectedException)
        {
            // Circuit disconnected during init; ignore
        }
        catch (Exception)
        {
            // Fallback: assume desktop viewport if JS interop unavailable
            _isMobile = false;
            IsInitialized = true;
        }
    }

    public void SetMode(AppViewportMode mode)
    {
        if (Mode == mode)
        {
            return;
        }

        Mode = mode;
        OnViewportChanged?.Invoke();
    }

    [JSInvokable]
    public void OnBrowserResize(bool isMobile)
    {
        if (_isMobile == isMobile)
        {
            return;
        }

        _isMobile = isMobile;
        OnViewportChanged?.Invoke();
    }

    public async ValueTask DisposeAsync()
    {
        if (_isDisposed)
        {
            return;
        }

        _isDisposed = true;
        try
        {
            await jsRuntime.InvokeVoidAsync("PassKeeInterop.disposeViewportListener");
        }
        catch
        {
            // Ignore during disposal
        }
        finally
        {
            _dotNetRef?.Dispose();
        }
    }
}

