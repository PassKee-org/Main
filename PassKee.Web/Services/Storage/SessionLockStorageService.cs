using System.Threading.Tasks;
using Microsoft.JSInterop;
using PassKee.Business.Common.Helpers;

namespace PassKee.Web.Services.Storage;

public class SessionLockStorageService : ISessionLockStorageService
{
    private const string SessionLockItem = "session_lock";
    private readonly IJSRuntime _jsRuntime;

    public SessionLockStorageService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task<SessionLockInfo?> GetSessionLockInfoAsync()
    {
        var json = await _jsRuntime.InvokeAsync<string?>("sessionStorage.getItem", SessionLockItem);
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        return JsonHelper.DeserializeObject<SessionLockInfo>(json);
    }

    public async Task SetSessionLockInfoAsync(SessionLockInfo info)
    {
        var json = JsonHelper.SerializeToString(info);
        await _jsRuntime.InvokeVoidAsync("sessionStorage.setItem", SessionLockItem, json);
    }

    public async Task ClearSessionLockInfoAsync()
    {
        await _jsRuntime.InvokeVoidAsync("sessionStorage.removeItem", SessionLockItem);
    }

    public async Task LockSessionAsync()
    {
        var info = await GetSessionLockInfoAsync();
        if (info != null)
        {
            info.IsLocked = true;
            await SetSessionLockInfoAsync(info);
        }
    }

    public async Task ClearAllSessionDataAsync()
    {
        await ClearSessionLockInfoAsync();
    }
}
