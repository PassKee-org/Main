using System;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Fluxor;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using PassKee.Web.Services.Storage;
using PassKee.Web.Store.Auth;
using PassKee.Web.Store.Vaults;

namespace PassKee.Web.Services.Auth;

public class InactivityLockService : IInactivityLockService
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromMinutes(60);
    private static readonly TimeSpan DefaultCheckInterval = TimeSpan.FromSeconds(10);

    private readonly IJSRuntime _jsRuntime;
    private readonly ISessionLockStorageService _sessionLockStorage;
    private readonly IState<AuthState> _authState;
    private readonly IState<VaultsState> _vaultsState;
    private readonly IDispatcher _dispatcher;
    private readonly NavigationManager _navigationManager;

    private DotNetObjectReference<InactivityLockService>? _dotNetRef;
    private Timer? _timer;
    private long _lastActivityTicks;
    private int _isLocking;
    private bool _isDisposed;
    private bool _isInitializing;
    private bool _isInitialized;

    public TimeSpan InactivityTimeout { get; set; } = DefaultTimeout;
    public TimeSpan CheckInterval { get; set; } = DefaultCheckInterval;
    public bool IsRunning => _timer != null && !_isDisposed;

    public InactivityLockService(
        IJSRuntime jsRuntime,
        ISessionLockStorageService sessionLockStorage,
        IState<AuthState> authState,
        IState<VaultsState> vaultsState,
        IDispatcher dispatcher,
        NavigationManager navigationManager)
    {
        _jsRuntime = jsRuntime;
        _sessionLockStorage = sessionLockStorage;
        _authState = authState;
        _vaultsState = vaultsState;
        _dispatcher = dispatcher;
        _navigationManager = navigationManager;

        Volatile.Write(ref _lastActivityTicks, DateTime.UtcNow.Ticks);
    }

    public void Initialize()
    {
        if (_isDisposed || _isInitialized || _isInitializing)
        {
            return;
        }

        _isInitializing = true;
        Volatile.Write(ref _lastActivityTicks, DateTime.UtcNow.Ticks);

        _timer = new Timer(OnTimerTick, null, CheckInterval, CheckInterval);

        _ = InitializeJsListenerAsync();
    }

    private async Task InitializeJsListenerAsync()
    {
        try
        {
            _dotNetRef = DotNetObjectReference.Create(this);
            await _jsRuntime.InvokeVoidAsync("PassKeeInterop.inactivityTracker.init", _dotNetRef);
            _isInitialized = true;
        }
        catch (JSDisconnectedException)
        {
            // Circuit disconnected or component shutting down
        }
        catch (InvalidOperationException)
        {
            // Prerendering or JS interop unavailable
        }
        finally
        {
            _isInitializing = false;
        }
    }

    [JSInvokable]
    public Task OnUserClickActivity()
    {
        Volatile.Write(ref _lastActivityTicks, DateTime.UtcNow.Ticks);
        return Task.CompletedTask;
    }

    public Task RecordActivityAsync()
    {
        Volatile.Write(ref _lastActivityTicks, DateTime.UtcNow.Ticks);
        return Task.CompletedTask;
    }

    private async void OnTimerTick(object? state)
    {
        try
        {
            await CheckInactivityAsync();
        }
        catch
        {
            // Suppress unhandled background exceptions from timer callback
        }
    }

    public async Task CheckInactivityAsync()
    {
        if (_isDisposed)
        {
            return;
        }

        var auth = _authState?.Value;
        if (auth == null)
        {
            return;
        }

        // Only lock if user is authenticated with active keys
        if (!auth.IsAuthenticated && auth.UserPrivateKey == null)
        {
            return;
        }

        var lastTicks = Volatile.Read(ref _lastActivityTicks);
        var elapsed = DateTime.UtcNow - new DateTime(lastTicks, DateTimeKind.Utc);

        if (elapsed >= InactivityTimeout)
        {
            await LockAsync();
        }
    }

    public async Task LockAsync()
    {
        if (Interlocked.Exchange(ref _isLocking, 1) != 0)
        {
            return;
        }

        // Stop timer
        _timer?.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);

        var auth = _authState?.Value;
        var vaults = _vaultsState?.Value;

        // Wipe sensitive cryptographic material from memory
        if (auth?.UserPrivateKey is { } userPrivateKey)
        {
            CryptographicOperations.ZeroMemory(userPrivateKey);
        }
        if (vaults?.ActiveVaultKey is { } activeVaultKey)
        {
            CryptographicOperations.ZeroMemory(activeVaultKey);
        }

        _dispatcher.Dispatch(new ResetAuthStateAction());
        _dispatcher.Dispatch(new ResetVaultsStateAction());

        await _sessionLockStorage.LockSessionAsync();

        // Navigate to /login (Lock Screen)
        _navigationManager.NavigateTo("/login");
    }

    public async ValueTask DisposeAsync()
    {
        if (_isDisposed)
        {
            return;
        }

        _isDisposed = true;

        if (_timer != null)
        {
            await _timer.DisposeAsync();
            _timer = null;
        }

        if (_dotNetRef != null)
        {
            try
            {
                await _jsRuntime.InvokeVoidAsync("PassKeeInterop.inactivityTracker.dispose");
            }
            catch
            {
                // Ignore JS cleanup errors during tear down
            }

            _dotRefDispose();
        }
    }

    private void _dotRefDispose()
    {
        _dotNetRef?.Dispose();
        _dotNetRef = null;
    }
}
