using Domain.Abstractions;

namespace PassKee.Business.Services.Http;

public interface IHttpTokenResolverService: IScopedDomainService
{
    string? GetApiToken();
    
    string? GetAccessToken();
}
