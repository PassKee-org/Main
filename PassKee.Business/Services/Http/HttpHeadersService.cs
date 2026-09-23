using System.Web;
using Microsoft.AspNetCore.Http;
using PassKee.Business.Common.Constants.Http;

namespace PassKee.Business.Services.Http;

public class HttpHeadersService: IHttpHeadersService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpHeadersService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }
    
    public void Append(HttpHeaderKeyEnum key, string value, DateTimeOffset? expires = null)
    {
        if (_httpContextAccessor.HttpContext == null)
            return;
        Append(_httpContextAccessor.HttpContext, key, value, expires);
    }
    
    public void Append(HttpContext context, HttpHeaderKeyEnum key, string value, DateTimeOffset? expires = null)
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
        context.Response.Headers.Append(name, HttpUtility.UrlEncode(value));
    }
    
    public string? Get(HttpHeaderKeyEnum key)
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
        context.Request.Headers.TryGetValue(name, out var value);
        return HttpUtility.UrlDecode(value);
    }
}
