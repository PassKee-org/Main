using System;
using System.Net;
using Domain.Abstractions.Api;
using Microsoft.AspNetCore.Http;
using PassKee.Business.Common.Exceptions;
using PassKee.Business.Services.Auth;

namespace PassKee.Business.Services.Http;

public class BaseApiRequestService: IBaseApiRequestService
{
    private readonly IHttpContextAccessor _httpContext;
    private readonly IJwtAuthService _jwtAuthService;
    private readonly IHttpTokenResolverService _httpTokenResolverService;

    private Guid? _currentUserGuid = null;
    
    public BaseApiRequestService(
        IHttpContextAccessor httpContext,
        IJwtAuthService jwtAuthService,
        IHttpTokenResolverService httpTokenResolverService
    )
    {
        _httpContext = httpContext;
        _jwtAuthService = jwtAuthService;
        _httpTokenResolverService = httpTokenResolverService;
    }
    
    public bool IsApiRequest()
    {
        return _httpContext?.HttpContext != null;
    }

    public bool IsTestMode()
    {
        throw new NotImplementedException();
    }

    public string? GetRequestUrl()
    {
        return _httpContext?.HttpContext?.Request.Path.Value?.ToLower();
    }
    
    public string? GetApiToken()
    {
        return _httpTokenResolverService.GetApiToken();
    }

    public string? GetAccessToken()
    {
        return _httpTokenResolverService.GetAccessToken();
    }
    
    public Guid? GetUserGuidFromJwt()
    {
        if (_currentUserGuid == null)
        {
            var user = _httpContext?.HttpContext?.User;
            if (user?.Identity?.IsAuthenticated == true)
            {
                var claim = user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier) ?? user.FindFirst("nameid");
                if (claim != null && Guid.TryParse(claim.Value, out var authGuid) && authGuid != Guid.Empty)
                {
                    _currentUserGuid = authGuid;
                    return _currentUserGuid;
                }
            }

            var jwtToken = GetApiToken();
            if (!string.IsNullOrEmpty(jwtToken))
            {
                var userId = _jwtAuthService.GetUserId(jwtToken);
                if (userId != Guid.Empty)
                {
                    _currentUserGuid = userId;
                }
            }
        }
        return _currentUserGuid;
    }
    
    public Guid? GetAccessTokenIdFromJwt()
    {
        var jwtToken = GetApiToken();
        if (string.IsNullOrEmpty(jwtToken))
        {
            return null;
        }

        var tokenId = _jwtAuthService.GetAccessTokenId(jwtToken);
        return tokenId == Guid.Empty ? null : tokenId;
    }
    
    public Guid GetCurrentUserId()
    {
        var guid = GetUserGuidFromJwt();
        if (guid == null || guid == Guid.Empty)
        {
            throw DomainException.UserNotFoundException;
        }
        return guid.Value;
    }

    public Guid? GetCurrentUserIdOrDefault()
    {
        return IsAuthorized() ? GetUserGuidFromJwt() : null;
    }
    
    public bool IsAuthorized()
    {
        var guid = GetUserGuidFromJwt();
        return guid != null && guid != Guid.Empty;
    }
    
    public IPAddress? GetIpAddress()
    {
        return _httpContext.HttpContext?.Connection.RemoteIpAddress;
    }
}
