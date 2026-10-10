using System;
using System.Threading;
using System.Threading.Tasks;
using Fluxor;
using Microsoft.JSInterop;
using Moq;
using PassKee.Tests.Unit.Web.Mocks;
using PassKee.Web.Services.Auth;
using PassKee.Web.Services.Storage;
using PassKee.Web.Store.Auth;
using PassKee.Web.Store.Vaults;
using Xunit;

namespace PassKee.Tests.Unit.Web.Services.Auth;

public class InactivityLockServiceTests
{
    private readonly Mock<IJSRuntime> _jsRuntimeMock;
    private readonly Mock<ISessionLockStorageService> _sessionLockStorageMock;
    private readonly Mock<IState<AuthState>> _authStateMock;
    private readonly Mock<IState<VaultsState>> _vaultsStateMock;
    private readonly Mock<IDispatcher> _dispatcherMock;
    private readonly TestNavigationManager _navigationManager;

    public InactivityLockServiceTests()
    {
        _jsRuntimeMock = new Mock<IJSRuntime>();
        _sessionLockStorageMock = new Mock<ISessionLockStorageService>();
        _authStateMock = new Mock<IState<AuthState>>();
        _vaultsStateMock = new Mock<IState<VaultsState>>();
        _dispatcherMock = new Mock<IDispatcher>();
        _navigationManager = new TestNavigationManager();
    }

    [Fact]
    public void DefaultInactivityTimeout_IsSixtyMinutes()
    {
        var service = new InactivityLockService(
            _jsRuntimeMock.Object,
            _sessionLockStorageMock.Object,
            _authStateMock.Object,
            _vaultsStateMock.Object,
            _dispatcherMock.Object,
            _navigationManager);

        Assert.Equal(TimeSpan.FromMinutes(60), service.InactivityTimeout);
    }

    [Fact]
    public async Task Initialize_StartsRunningTimer()
    {
        _authStateMock.Setup(s => s.Value).Returns(new AuthState());
        _vaultsStateMock.Setup(s => s.Value).Returns(new VaultsState());

        var service = new InactivityLockService(
            _jsRuntimeMock.Object,
            _sessionLockStorageMock.Object,
            _authStateMock.Object,
            _vaultsStateMock.Object,
            _dispatcherMock.Object,
            _navigationManager);

        Assert.False(service.IsRunning);

        service.Initialize();

        Assert.True(service.IsRunning);

        await service.DisposeAsync();
    }

    [Fact]
    public async Task LockAsync_ClearsKeysDispatchesResetsLocksSessionAndNavigatesToLogin()
    {
        var privateKey = new byte[] { 1, 2, 3, 4, 5 };
        var vaultKey = new byte[] { 6, 7, 8, 9, 10 };

        _authStateMock.Setup(s => s.Value).Returns(new AuthState
        {
            IsAuthenticated = true,
            UserPrivateKey = privateKey
        });

        _vaultsStateMock.Setup(s => s.Value).Returns(new VaultsState
        {
            ActiveVaultKey = vaultKey
        });

        var service = new InactivityLockService(
            _jsRuntimeMock.Object,
            _sessionLockStorageMock.Object,
            _authStateMock.Object,
            _vaultsStateMock.Object,
            _dispatcherMock.Object,
            _navigationManager);

        await service.LockAsync();

        // Verify keys were wiped with ZeroMemory
        Assert.All(privateKey, b => Assert.Equal(0, b));
        Assert.All(vaultKey, b => Assert.Equal(0, b));

        // Verify dispatching resets
        _dispatcherMock.Verify(d => d.Dispatch(It.IsAny<ResetAuthStateAction>()), Times.Once);
        _dispatcherMock.Verify(d => d.Dispatch(It.IsAny<ResetVaultsStateAction>()), Times.Once);

        // Verify session storage lock
        _sessionLockStorageMock.Verify(s => s.LockSessionAsync(), Times.Once);

        // Verify navigation to /login
        Assert.Equal("/login", _navigationManager.NavigatedToUri);
    }

    [Fact]
    public async Task OnUserClickActivity_ResetsInactivityTimer()
    {
        _authStateMock.Setup(s => s.Value).Returns(new AuthState
        {
            IsAuthenticated = true,
            UserPrivateKey = new byte[] { 1 }
        });
        _vaultsStateMock.Setup(s => s.Value).Returns(new VaultsState());

        var service = new InactivityLockService(
            _jsRuntimeMock.Object,
            _sessionLockStorageMock.Object,
            _authStateMock.Object,
            _vaultsStateMock.Object,
            _dispatcherMock.Object,
            _navigationManager);

        service.InactivityTimeout = TimeSpan.FromMilliseconds(50);
        service.CheckInterval = TimeSpan.FromMilliseconds(10);
        service.Initialize();

        // Activity occurs periodically
        for (int i = 0; i < 5; i++)
        {
            await Task.Delay(20);
            await service.OnUserClickActivity();
            await service.CheckInactivityAsync();
        }

        // Lock should not have been called because activity kept resetting it
        _sessionLockStorageMock.Verify(s => s.LockSessionAsync(), Times.Never);
        Assert.Null(_navigationManager.NavigatedToUri);

        // Now wait past the timeout without activity
        await Task.Delay(60);
        await service.CheckInactivityAsync();

        _sessionLockStorageMock.Verify(s => s.LockSessionAsync(), Times.Once);
        Assert.Equal("/login", _navigationManager.NavigatedToUri);

        await service.DisposeAsync();
    }

    [Fact]
    public async Task CheckInactivityAsync_DoesNotLock_WhenUserIsNotAuthenticated()
    {
        _authStateMock.Setup(s => s.Value).Returns(new AuthState
        {
            IsAuthenticated = false,
            UserPrivateKey = null
        });

        var service = new InactivityLockService(
            _jsRuntimeMock.Object,
            _sessionLockStorageMock.Object,
            _authStateMock.Object,
            _vaultsStateMock.Object,
            _dispatcherMock.Object,
            _navigationManager);

        service.InactivityTimeout = TimeSpan.FromMilliseconds(10);
        await Task.Delay(20);
        await service.CheckInactivityAsync();

        _sessionLockStorageMock.Verify(s => s.LockSessionAsync(), Times.Never);
        _dispatcherMock.Verify(d => d.Dispatch(It.IsAny<ResetAuthStateAction>()), Times.Never);
        Assert.Null(_navigationManager.NavigatedToUri);
    }

    [Fact]
    public async Task DisposeAsync_CleansUpTimerAndDotNetRef()
    {
        var service = new InactivityLockService(
            _jsRuntimeMock.Object,
            _sessionLockStorageMock.Object,
            _authStateMock.Object,
            _vaultsStateMock.Object,
            _dispatcherMock.Object,
            _navigationManager);

        service.Initialize();
        Assert.True(service.IsRunning);

        await service.DisposeAsync();

        Assert.False(service.IsRunning);
    }
}
