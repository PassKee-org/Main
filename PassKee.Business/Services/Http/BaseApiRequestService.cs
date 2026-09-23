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
            var jwtToken = GetApiToken();
            _currentUserGuid = string.IsNullOrEmpty(jwtToken) ? null : _jwtAuthService.GetUserId(jwtToken);
        }
        return _currentUserGuid;
    }
    
    public Guid? GetAccessTokenIdFromJwt()
    {
        var jwtToken = GetApiToken();
        return string.IsNullOrEmpty(jwtToken) ? null : _jwtAuthService.GetAccessTokenId(jwtToken);
    }
    
    public Guid GetCurrentUserId()
    {
        var guid = GetUserGuidFromJwt();
        if (guid == null)
        {
            throw DomainException.UserNotFoundException;
        }
        return guid.Value;
    }
    
    public bool IsAuthorized()
    {
        return GetUserGuidFromJwt() != null;
    }
    
    public IPAddress? GetIpAddress()
    {
        return _httpContext.HttpContext?.Connection.RemoteIpAddress;
    }
}
