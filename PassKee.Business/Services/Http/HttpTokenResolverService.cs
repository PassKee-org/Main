using Microsoft.AspNetCore.Http;
using PassKee.Business.Common.Constants.Http;

namespace PassKee.Business.Services.Http;

public class HttpTokenResolverService: IHttpTokenResolverService
{
    private readonly IHttpCookiesService _httpCookiesService;
    private readonly IHttpContextAccessor? _httpContextAccessor;
    private readonly IHttpHeadersService _httpHeadersService;

    public HttpTokenResolverService(
        IHttpCookiesService httpCookiesService,
        IHttpContextAccessor httpContextAccessor,
        IHttpHeadersService httpHeadersService
    )
    {
        _httpCookiesService = httpCookiesService;
        _httpContextAccessor = httpContextAccessor;
        _httpHeadersService = httpHeadersService;
    }

    public string? GetApiToken()
    {
        if (_httpContextAccessor?.HttpContext == null)
            return null;
        var request = _httpContextAccessor!.HttpContext!.Request;
        string? authToken = null;
        authToken = _httpHeadersService.Get(HttpHeaderKeyEnum.JwtToken);
        if (string.IsNullOrEmpty(authToken) && request.Headers.ContainsKey("Authorization"))
        {
            authToken = request.Headers["Authorization"].FirstOrDefault()!;
            if (!string.IsNullOrEmpty(authToken) && authToken.StartsWith("Bearer "))
            {
                authToken = authToken.Substring(7);
            }
            else
            {
                authToken = null;
            }
        }
        if (string.IsNullOrEmpty(authToken))
        {
            authToken = _httpCookiesService.Get(HttpCookieKeyEnum.JwtToken);
        }
        if (!string.IsNullOrEmpty(authToken))
        {
            authToken = Uri.UnescapeDataString(authToken);
        }
        return authToken;
    }
    
    public string? GetAccessToken()
    {
        if (_httpContextAccessor?.HttpContext == null)
            return null;
        string? authToken = _httpHeadersService.Get(HttpHeaderKeyEnum.AccessToken);
        if (string.IsNullOrEmpty(authToken))
        {
            authToken = _httpCookiesService.Get(HttpCookieKeyEnum.AccessToken);
        }
        if (!string.IsNullOrEmpty(authToken))
        {
            authToken = Uri.UnescapeDataString(authToken);
        }
        return authToken;
    }
}
