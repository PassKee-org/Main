using System.Net;
using System.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using PassKee.Business.Common.Constants.Http;
using PassKee.Business.Helpers;

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
        name = PrepareName(name, context);
        var isProduction = ApplicationHelper.HostingEnvironment == "Production";
        var isHttps = context.Request.IsHttps;
        var host = context.Request.Host.Host;
        var isLocal = host is "localhost" or "127.0.0.1" || IPAddress.TryParse(host, out _);

        var cookieDomains = (isLocal || _cookieDomains.Length == 0) ? [string.Empty] : _cookieDomains;
        foreach (var cookieDomain in cookieDomains)
        {
            var targetDomain = ResolveCookieDomain(host, isLocal, cookieDomain);
            var cookieOptions = new CookieOptions
            {
                Domain = targetDomain,
                HttpOnly = true,
                Secure = isProduction || isHttps,
                SameSite = isProduction
                    ? SameSiteMode.Lax
                    : (isHttps ? SameSiteMode.None : SameSiteMode.Lax),
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
        var preparedName = PrepareName(name, context);
        if (context.Request.Cookies.TryGetValue(preparedName, out var value))
        {
            return HttpUtility.UrlDecode(value);
        }

        if (preparedName != name && context.Request.Cookies.TryGetValue(name, out value))
        {
            return HttpUtility.UrlDecode(value);
        }

        return null;
    }

    public string PrepareName(HttpCookieKeyEnum key)
    {
        return PrepareName(key.GetKey());
    }

    public string PrepareName(string name)
    {
        return PrepareName(name, _httpContextAccessor.HttpContext);
    }

    public string PrepareName(string name, HttpContext? context)
    {
        var host = context?.Request.Host.Host ?? _httpContextAccessor.HttpContext?.Request.Host.Host;
        var isLocal = host == null || host is "localhost" or "127.0.0.1" || IPAddress.TryParse(host, out _);

        if (!isLocal && !string.IsNullOrEmpty(_cookieKeyPostfix))
        {
            return $"{name}_{_cookieKeyPostfix}";
        }
        return name;
    }

    private static string? ResolveCookieDomain(string host, bool isLocal, string? configuredDomain)
    {
        if (isLocal || string.IsNullOrEmpty(configuredDomain))
        {
            return null;
        }

        var trimmedDomain = configuredDomain.TrimStart('.');
        if (host.Equals(trimmedDomain, StringComparison.OrdinalIgnoreCase) ||
            host.EndsWith("." + trimmedDomain, StringComparison.OrdinalIgnoreCase))
        {
            return configuredDomain;
        }

        return null;
    }
}
