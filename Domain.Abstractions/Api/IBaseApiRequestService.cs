using System;
using System.Net;

namespace Domain.Abstractions.Api;

public interface IBaseApiRequestService: IDomainService
{
    bool IsApiRequest();
    
    string? GetAccessToken();
    
    string? GetApiToken();

    Guid? GetUserGuidFromJwt();

    Guid? GetAccessTokenIdFromJwt();

    Guid GetCurrentUserId();

    bool IsAuthorized();
    
    string? GetRequestUrl();
    
    IPAddress? GetIpAddress();
}
