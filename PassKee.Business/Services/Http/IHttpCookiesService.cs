using Domain.Abstractions;
using Microsoft.AspNetCore.Http;
using PassKee.Business.Common.Constants.Http;

namespace PassKee.Business.Services.Http;

public interface IHttpCookiesService: IScopedDomainService
{
    void Append(string name, string value, DateTimeOffset? expires = null);
    
    void Append(HttpContext context, string name, string value, DateTimeOffset? expires = null);
    
    void Append(HttpCookieKeyEnum key, string value, DateTimeOffset? expires = null);
    
    void Append(HttpContext context, HttpCookieKeyEnum key, string value, DateTimeOffset? expires = null);
    
    string? Get(HttpContext context, string name);
    
    string? Get(string name);
    
    string? Get(HttpCookieKeyEnum key);
    
    void AppendAuthCookies(string accessToken, string jwtToken);

    void CleanUpAuthCookies();

    string PrepareName(string name);

    string PrepareName(HttpCookieKeyEnum key);
}
