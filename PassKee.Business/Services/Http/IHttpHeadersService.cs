using Domain.Abstractions;
using Microsoft.AspNetCore.Http;
using PassKee.Business.Common.Constants.Http;

namespace PassKee.Business.Services.Http;

public interface IHttpHeadersService: IScopedDomainService
{
    void Append(string name, string value, DateTimeOffset? expires = null);
    
    void Append(HttpContext context, string name, string value, DateTimeOffset? expires = null);
    
    void Append(HttpHeaderKeyEnum key, string value, DateTimeOffset? expires = null);
    
    void Append(HttpContext context, HttpHeaderKeyEnum key, string value, DateTimeOffset? expires = null);
    
    string? Get(HttpContext context, string name);
    
    string? Get(string name);
    
    string? Get(HttpHeaderKeyEnum key);
}
