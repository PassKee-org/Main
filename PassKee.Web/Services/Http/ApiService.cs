using System.Net.Http;
using System.Threading.Tasks;
using PassKee.Api.Shared.Constants;
using PassKee.Web.Services.Http.Client;

namespace PassKee.Web.Services.Http;

public partial class ApiService
{
    private readonly CustomHttpClient _httpClient;

    public ApiService(CustomHttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<TResponse?> PostAsync<TResponse>(string requestUri, object? data = null)
    {
        return await _httpClient.RequestAsync<TResponse>(requestUri, data, HttpMethod.Post);
    }

    public async Task<TResponse?> GetAsync<TResponse>(string requestUri)
    {
        return await _httpClient.RequestAsync<TResponse>(requestUri, null, HttpMethod.Get);
    }

    public async Task<string?> PingAsync()
    {
        return await _httpClient.RequestAsync(ApiUrl.Ping, null, HttpMethod.Get);
    }
    public async Task<TResponse?> PutAsync<TResponse>(string requestUri, object? data = null)
    {
        return await _httpClient.RequestAsync<TResponse>(requestUri, data, HttpMethod.Put);
    }

    public async Task<TResponse?> DeleteAsync<TResponse>(string requestUri)
    {
        return await _httpClient.RequestAsync<TResponse>(requestUri, null, HttpMethod.Delete);
    }
}
