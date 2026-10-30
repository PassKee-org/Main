using System.Threading.Tasks;

namespace PassKee.Web.Services.Storage;

public class SessionLockStorageService : ISessionLockStorageService
{
    private const string SessionLockItem = "session_lock";
    private readonly ILocalStorageStoreService _storageStore;

    public SessionLockStorageService(ILocalStorageStoreService storageStore)
    {
        _storageStore = storageStore;
    }

    public async Task<SessionLockInfo?> GetSessionLockInfoAsync()
    {
        return await _storageStore.GetItemAsync<SessionLockInfo>(SessionLockItem);
    }

    public async Task SetSessionLockInfoAsync(SessionLockInfo info)
    {
        await _storageStore.SetItemAsync(SessionLockItem, info);
    }

    public async Task ClearSessionLockInfoAsync()
    {
        await _storageStore.RemoveItemAsync(SessionLockItem);
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
