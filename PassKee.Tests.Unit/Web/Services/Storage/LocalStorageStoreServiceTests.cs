using System.Threading;
using System.Threading.Tasks;
using Microsoft.JSInterop;
using Microsoft.JSInterop.Infrastructure;
using Moq;
using PassKee.Business.Common.Helpers;
using PassKee.Web.Services.Storage;
using Xunit;

namespace PassKee.Tests.Unit.Web.Services.Storage;

public class LocalStorageStoreServiceTests
{
    private readonly Mock<IJSRuntime> _jsMock = new();

    private LocalStorageStoreService CreateService() => new(_jsMock.Object);

    [Fact]
    public async Task GetItemAsync_InvokesLocalStorageGetItem()
    {
        _jsMock.Setup(js => js.InvokeAsync<string?>("localStorage.getItem", It.IsAny<object?[]>()))
            .ReturnsAsync("test_value");

        var service = CreateService();
        var result = await service.GetItemAsync("my_key");

        Assert.Equal("test_value", result);
        _jsMock.Verify(js => js.InvokeAsync<string?>("localStorage.getItem", It.Is<object?[]>(args => (string)args![0]! == "my_key")), Times.Once);
    }

    [Fact]
    public async Task GetItemAsync_ReturnsNull_WhenInvokeThrows()
    {
        _jsMock.Setup(js => js.InvokeAsync<string?>("localStorage.getItem", It.IsAny<object?[]>()))
            .ThrowsAsync(new JSException("Access denied"));

        var service = CreateService();
        var result = await service.GetItemAsync("my_key");

        Assert.Null(result);
    }

    [Fact]
    public async Task SetItemAsync_InvokesLocalStorageSetItem()
    {
        _jsMock.Setup(js => js.InvokeAsync<IJSVoidResult>("localStorage.setItem", It.IsAny<object?[]>()))
            .ReturnsAsync(Mock.Of<IJSVoidResult>());

        var service = CreateService();
        await service.SetItemAsync("my_key", "my_value");

        _jsMock.Verify(js => js.InvokeAsync<IJSVoidResult>("localStorage.setItem", It.Is<object?[]>(args => (string)args![0]! == "my_key" && (string)args[1]! == "my_value")), Times.Once);
    }

    [Fact]
    public async Task RemoveItemAsync_InvokesLocalStorageRemoveItem()
    {
        _jsMock.Setup(js => js.InvokeAsync<IJSVoidResult>("localStorage.removeItem", It.IsAny<object?[]>()))
            .ReturnsAsync(Mock.Of<IJSVoidResult>());

        var service = CreateService();
        await service.RemoveItemAsync("my_key");

        _jsMock.Verify(js => js.InvokeAsync<IJSVoidResult>("localStorage.removeItem", It.Is<object?[]>(args => (string)args![0]! == "my_key")), Times.Once);
    }

    [Fact]
    public async Task ClearAsync_InvokesLocalStorageClear()
    {
        _jsMock.Setup(js => js.InvokeAsync<IJSVoidResult>("localStorage.clear", It.IsAny<object?[]>()))
            .ReturnsAsync(Mock.Of<IJSVoidResult>());

        var service = CreateService();
        await service.ClearAsync();

        _jsMock.Verify(js => js.InvokeAsync<IJSVoidResult>("localStorage.clear", It.IsAny<object?[]>()), Times.Once);
    }

    [Fact]
    public async Task GenericGetAndSetItemAsync_SerializesAndDeserializesCorrectly()
    {
        var expected = new SessionLockInfo
        {
            Email = "user@test.com",
            IsLocked = true
        };
        var json = JsonHelper.SerializeToString(expected);

        _jsMock.Setup(js => js.InvokeAsync<string?>("localStorage.getItem", It.IsAny<object?[]>()))
            .ReturnsAsync(json);
        _jsMock.Setup(js => js.InvokeAsync<IJSVoidResult>("localStorage.setItem", It.IsAny<object?[]>()))
            .ReturnsAsync(Mock.Of<IJSVoidResult>());

        var service = CreateService();

        var retrieved = await service.GetItemAsync<SessionLockInfo>("session_lock");
        Assert.NotNull(retrieved);
        Assert.Equal("user@test.com", retrieved.Email);
        Assert.True(retrieved.IsLocked);

        await service.SetItemAsync("session_lock", expected);
        _jsMock.Verify(js => js.InvokeAsync<IJSVoidResult>("localStorage.setItem", It.Is<object?[]>(args => (string)args![0]! == "session_lock" && ((string)args[1]!).Contains("user@test.com"))), Times.Once);
    }
}

