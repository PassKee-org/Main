using System;
using System.Threading.Tasks;
using Autofac;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PassKee.Business.Common.Constants.Http;
using PassKee.Business.Common.Exceptions.Api.Auth;
using PassKee.Business.Services.Auth;
using PassKee.Business.Services.Http;
using PassKee.Orm.Dao;
using PassKee.Orm.Entities;

namespace PassKee.Business.Mvc.Middleware;

public class JwtRefreshMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<JwtRefreshMiddleware> _logger;
    private readonly TimeSpan _jwtExpirationDelay = TimeSpan.FromMinutes(5);

    public JwtRefreshMiddleware(
        RequestDelegate next,
        ILogger<JwtRefreshMiddleware> logger,
        IConfiguration configuration
    )
    {
        _next = next;
        _logger = logger;
        var jwtLifetime = TimeSpan.FromMinutes(configuration.GetValue("App:Auth:JwtLifetime", 60));
        var jwtRefreshDelay = TimeSpan.FromMinutes(configuration.GetValue("App:Auth:JwtRefreshDelay", 5));

        // Do not refresh every request when the configured window exceeds the token lifetime.
        _jwtExpirationDelay = jwtRefreshDelay < jwtLifetime
            ? jwtRefreshDelay
            : TimeSpan.FromTicks(jwtLifetime.Ticks / 2);
    }

    public async Task InvokeAsync(HttpContext context, ILifetimeScope scope)
    {
        try
        {
            var jwtAuthService = scope.Resolve<IJwtAuthService>();
            var authService = scope.Resolve<IAuthService>();
            var apiRequestService = scope.Resolve<IApiRequestService>();
            var cookiesService = scope.Resolve<IHttpCookiesService>();
            var accessTokenDao = scope.Resolve<IUserAccessTokenDao>();

            var jwt = apiRequestService.GetApiToken()
                      ?? cookiesService.Get(context, HttpCookieKeyEnum.JwtToken.GetKey());

            if (!string.IsNullOrEmpty(jwt) && jwtAuthService.IsJwt(jwt))
            {
                var accessToken = apiRequestService.GetAccessToken()
                                  ?? cookiesService.Get(context, HttpCookieKeyEnum.AccessToken.GetKey());

                if (jwtAuthService.IsValidJwt(jwt, false))
                {
                    UserAccessTokenEntity? accessTokenEntity = null;
                    if (!string.IsNullOrEmpty(accessToken))
                    {
                        accessTokenEntity = await accessTokenDao.GetByToken(accessToken);
                    }
                    else
                    {
                        var accessTokenId = jwtAuthService.GetAccessTokenId(jwt);
                        if (accessTokenId.HasValue)
                        {
                            accessTokenEntity = await accessTokenDao.GetById(accessTokenId.Value);
                        }
                    }

                    if (accessTokenEntity == null || accessTokenEntity.IsExpired)
                    {
                        _logger.LogDebug("Access token is missing, invalid, or expired in database.");
                        context.Items["__AuthFailed"] = "Access token is missing, invalid, or expired in database.";
                        context.Request.Headers.Remove("Authorization");
                        context.Request.Headers.Remove(HttpHeaderKeyEnum.JwtToken.ToString());
                    }
                    else
                    {
                        var jwtUserId = jwtAuthService.GetUserId(jwt);
                        if (accessTokenEntity.User.Id != jwtUserId)
                        {
                            _logger.LogWarning("Access token user does not match JWT user.");
                            context.Items["__AuthFailed"] = "Access token user does not match JWT user.";
                            context.Request.Headers.Remove("Authorization");
                        }
                        else
                        {
                            context.Request.Headers.Authorization = $"Bearer {jwt}";

                            if (jwtAuthService.IsTokenExpired(jwt, _jwtExpirationDelay))
                            {
                                _logger.LogDebug("Refresh JWT token...");
                                var authResult = await authService.GenerateNewJwtToken(accessTokenEntity);
                                var refreshedJwt = authResult.JwtToken;

                                var httpHeadersService = scope.Resolve<IHttpHeadersService>();

                                _logger.LogDebug(
                                    "Cookie with JWT token was refreshed. Expiration time: {Exp}",
                                    jwtAuthService.GetTokenExpirationTime(refreshedJwt)
                                );
                                context.Request.Headers.Authorization = $"Bearer {refreshedJwt}";

                                cookiesService.Append(
                                    context,
                                    HttpCookieKeyEnum.JwtToken,
                                    refreshedJwt,
                                    new DateTimeOffset(DateTime.SpecifyKind(accessTokenEntity.ExpirationTime, DateTimeKind.Utc))
                                );
                                httpHeadersService.Append(HttpHeaderKeyEnum.JwtToken, refreshedJwt);
                            }
                        }
                    }
                }
            }
        }
        catch (InvalidTokenException e)
        {
            _logger.LogTrace(e, "{Message}", e.Message);
        }
        catch (IncorrectAccessTokenException e)
        {
            _logger.LogTrace(e, "{Message}", e.Message);
        }
        catch (ExpiredJwtTokenException e)
        {
            _logger.LogTrace(e, "{Message}", e.Message);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "{Message}", e.Message);
        }
        finally
        {
            await _next(context);
        }
    }
}
