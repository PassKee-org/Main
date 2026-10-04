using System.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using PassKee.Business.Common.Constants.Http;

namespace PassKee.Business.Services.Http;

public class HttpCookiesService: IHttpCookiesService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly string[] _cookieDomains;
    private readonly int _accessTokenLifeTime;
    private readonly string _cookieKeyPostfix;

    public HttpCookiesService(
        IConfiguration configuration,
        IHttpContextAccessor httpContextAccessor
    )
    {
        _httpContextAccessor = httpContextAccessor;
        _cookieDomains = configuration.GetSection("App:Auth:CookieDomains").Get<string[]>() ?? [];
        _cookieKeyPostfix = configuration.GetValue<string>("App:Auth:CookieKeyPostfix") ?? string.Empty;
        _accessTokenLifeTime = configuration.GetValue<int>("App:Auth:AccessTokenLifetime", 60);
    }
    
    public void AppendAuthCookies(string accessToken, string jwtToken)
    {
        var accessTokenTimeSpan = DateTimeOffset.UtcNow.AddMinutes(_accessTokenLifeTime);
        
        Append(HttpCookieKeyEnum.AccessToken, accessToken, accessTokenTimeSpan);
        Append(HttpCookieKeyEnum.JwtToken, jwtToken, accessTokenTimeSpan);
    }
    
    public void CleanUpAuthCookies()
    {
        Append(HttpCookieKeyEnum.JwtToken, string.Empty, DateTimeOffset.UtcNow);
        Append(HttpCookieKeyEnum.AccessToken, string.Empty, DateTimeOffset.UtcNow);
    }
    
    public void Append(HttpCookieKeyEnum key, string value, DateTimeOffset? expires = null)
    {
        if (_httpContextAccessor.HttpContext == null)
            return;
        Append(_httpContextAccessor.HttpContext, key, value, expires);
    }
    
    public void Append(HttpContext context, HttpCookieKeyEnum key, string value, DateTimeOffset? expires = null)
    {
        Append(context, key.GetKey(), value, expires);
    }
    
    public void Append(string name, string value, DateTimeOffset? expires = null)
    {
        if (_httpContextAccessor.HttpContext == null)
            return;
        Append(_httpContextAccessor.HttpContext, name, value, expires);
    }
    
    public void Append(HttpContext context, string name, string value, DateTimeOffset? expires = null)
    {
        name = PrepareName(name);
        var cookieDomains = _cookieDomains.Length == 0 ? [string.Empty] : _cookieDomains;
        foreach (var cookieDomain in cookieDomains)
        {
            var cookieOptions = new CookieOptions
            {
                Domain = string.IsNullOrEmpty(cookieDomain) ? null : cookieDomain,
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = expires
            };
            context.Response.Cookies.Append(
                name,
                HttpUtility.UrlEncode(value),
                cookieOptions
            );    
        }
    }
    
    public string? Get(HttpCookieKeyEnum key)
    {
        return Get(key.GetKey());
    }
    
    public string? Get(string name)
    {
        if (_httpContextAccessor.HttpContext == null)
            return null;
        return Get(_httpContextAccessor.HttpContext, name);
    }
    
    public string? Get(HttpContext context, string name)
    {
        name = PrepareName(name);
        context.Request.Cookies.TryGetValue(name, out var value);
        return HttpUtility.UrlDecode(value);
    }

    public string PrepareName(HttpCookieKeyEnum key)
    {
        return PrepareName(key.GetKey());
    }

    public string PrepareName(string name)
    {
        if (!string.IsNullOrEmpty(_cookieKeyPostfix))
        {
            return $"{name}_{_cookieKeyPostfix}";
        }
        return name;
    }
}
