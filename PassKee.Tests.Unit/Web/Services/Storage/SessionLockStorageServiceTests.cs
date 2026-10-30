using System.Threading.Tasks;
using Moq;
using PassKee.Web.Services.Storage;
using Xunit;

namespace PassKee.Tests.Unit.Web.Services.Storage;

public class SessionLockStorageServiceTests
{
    private readonly Mock<ILocalStorageStoreService> _storageMock = new();

    private SessionLockStorageService CreateService() => new(_storageMock.Object);

    [Fact]
    public async Task GetSessionLockInfoAsync_ReturnsFromLocalStorage_WhenPresent()
    {
        var expected = new SessionLockInfo
        {
            Email = "test@example.com",
            EncryptedSecretKeyBase64 = "encrypted_key",
            SessionUnlockSaltBase64 = "salt_123",
            FormatVersion = SessionLockInfo.CurrentFormatVersion
        };

        _storageMock.Setup(s => s.GetItemAsync<SessionLockInfo>("session_lock"))
            .ReturnsAsync(expected);

        var service = CreateService();
        var result = await service.GetSessionLockInfoAsync();

        Assert.NotNull(result);
        Assert.Equal("test@example.com", result.Email);
        Assert.Equal("encrypted_key", result.EncryptedSecretKeyBase64);
        Assert.Equal(SessionLockInfo.CurrentFormatVersion, result.FormatVersion);
    }

    [Fact]
    public async Task GetSessionLockInfoAsync_ReturnsNull_WhenEmpty()
    {
        _storageMock.Setup(s => s.GetItemAsync<SessionLockInfo>("session_lock"))
            .ReturnsAsync((SessionLockInfo?)null);

        var service = CreateService();
        var result = await service.GetSessionLockInfoAsync();

        Assert.Null(result);
    }

    [Fact]
    public async Task SetSessionLockInfoAsync_SavesToLocalStorage()
    {
        var info = new SessionLockInfo
        {
            Email = "sync@example.com",
            EncryptedSecretKeyBase64 = "key_sync"
        };

        var service = CreateService();
        await service.SetSessionLockInfoAsync(info);

        _storageMock.Verify(s => s.SetItemAsync("session_lock", info), Times.Once);
    }

    [Fact]
    public async Task ClearSessionLockInfoAsync_RemovesFromLocalStorage()
    {
        var service = CreateService();
        await service.ClearSessionLockInfoAsync();

        _storageMock.Verify(s => s.RemoveItemAsync("session_lock"), Times.Once);
    }

    [Fact]
    public async Task LockSessionAsync_SetsIsLockedToTrue()
    {
        var info = new SessionLockInfo
        {
            Email = "lock@example.com",
            IsLocked = false
        };

        _storageMock.Setup(s => s.GetItemAsync<SessionLockInfo>("session_lock"))
            .ReturnsAsync(info);

        var service = CreateService();
        await service.LockSessionAsync();

        _storageMock.Verify(s => s.SetItemAsync("session_lock", It.Is<SessionLockInfo>(i => i.IsLocked && i.Email == "lock@example.com")), Times.Once);
    }
}
