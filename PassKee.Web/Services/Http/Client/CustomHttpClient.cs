using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace PassKee.Web.Services.Http.Client;

public class CustomHttpClient
{
    private readonly string _apiUrl;
    private readonly HttpClient _httpClient;
    private readonly ILogger<CustomHttpClient> _logger;

    public CustomHttpClient(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<CustomHttpClient> logger
    )
    {
        _httpClient = httpClient;
        _logger = logger;
        _apiUrl = (configuration.GetValue<string>("ApiUrl") ?? "").TrimEnd('/');
    }

    public async Task<TResponse?> RequestAsync<TResponse>(string requestUri, object? data, HttpMethod httpMethod)
    {
        var responseString = await RequestAsync(requestUri, data, httpMethod);
        return Deserialize<TResponse>(responseString);
    }

    public async Task<string> RequestAsync(string requestUri, object? data, HttpMethod httpMethod)
    {
        var fullUri = string.IsNullOrEmpty(_apiUrl) ? requestUri : $"{_apiUrl}/{requestUri.TrimStart('/')}";
        var request = new HttpRequestMessage(httpMethod, fullUri);
        request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);

        if (data != null && (httpMethod == HttpMethod.Post || httpMethod == HttpMethod.Put))
        {
            var json = JsonConvert.SerializeObject(data);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
        }

        var response = await _httpClient.SendAsync(request);
        return await HandleHttpResponse(response);
    }

    private async Task<string> HandleHttpResponse(HttpResponseMessage response)
    {
        var responseString = await response.Content.ReadAsStringAsync();
        if (response.IsSuccessStatusCode)
        {
            return responseString;
        }

        _logger.LogError($"HTTP request failed: {response.StatusCode}, Content: {responseString}");
        response.EnsureSuccessStatusCode();
        return responseString;
    }

    private T? Deserialize<T>(string responseString)
    {
        try
        {
            return JsonConvert.DeserializeObject<T>(responseString);
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            return default;
        }
    }
}

