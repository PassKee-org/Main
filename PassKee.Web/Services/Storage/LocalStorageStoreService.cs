using System.Threading.Tasks;
using Microsoft.JSInterop;
using PassKee.Business.Common.Helpers;

namespace PassKee.Web.Services.Storage;

public class LocalStorageStoreService : ILocalStorageStoreService
{
    private readonly IJSRuntime _jsRuntime;

    public LocalStorageStoreService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task<string?> GetItemAsync(string key)
    {
        try
        {
            return await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", key);
        }
        catch
        {
            return null;
        }
    }

    public async Task SetItemAsync(string key, string value)
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", key, value);
        }
        catch
        {
            // Suppress storage write failures in restrictive sandboxes or full quota
        }
    }

    public async Task RemoveItemAsync(string key)
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", key);
        }
        catch
        {
            // Suppress storage removal errors
        }
    }

    public async Task ClearAsync()
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.clear");
        }
        catch
        {
            // Suppress clear errors
        }
    }

    public async Task<T?> GetItemAsync<T>(string key)
    {
        var json = await GetItemAsync(key);
        if (string.IsNullOrWhiteSpace(json))
        {
            return default;
        }

        try
        {
            return JsonHelper.DeserializeObject<T>(json);
        }
        catch
        {
            return default;
        }
    }

    public Task SetItemAsync<T>(string key, T value)
    {
        if (value is null)
        {
            return RemoveItemAsync(key);
        }

        var json = JsonHelper.SerializeToString(value);
        return SetItemAsync(key, json);
    }
}
