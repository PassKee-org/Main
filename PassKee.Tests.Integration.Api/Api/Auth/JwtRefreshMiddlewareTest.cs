using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NHibernate.Linq;
using PassKee.Api.Shared.Constants;
using PassKee.Api.Shared.Dto.RequestsAndResponses.Auth;
using PassKee.Business.Common.Constants.Http;
using PassKee.Business.Common.Utils;
using PassKee.Business.Services.Auth;
using PassKee.Business.Testing.Extensions;
using PassKee.Orm.Dao;
using PassKee.Orm.Entities;
using PassKee.Tests.Integration.Api.Core;
using Xunit;

namespace PassKee.Tests.Integration.Api.Api.Auth;

public class JwtRefreshMiddlewareTest : BaseTest
{
    private readonly IAuthService _authService;
    private readonly IJwtAuthService _jwtService;
    private readonly IUserAccessTokenDao _accessTokenDao;

    public JwtRefreshMiddlewareTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        _authService = ServiceProvider.GetRequiredService<IAuthService>();
        _jwtService = ServiceProvider.GetRequiredService<IJwtAuthService>();
        _accessTokenDao = ServiceProvider.GetRequiredService<IUserAccessTokenDao>();
    }

    [Fact]
    public async Task ShouldRefreshJwtTokenFromCookiesWhenExpiringSoon()
    {
        var email = $"refresh_expiring_{Guid.NewGuid():N}@example.com";
        var regData = CryptoUtils.PrepareClientRegistration("Password#2026");

        var regResponse = await PostRequestAsAnonymousAsync(ApiUrl.AuthRegister, new RegisterRequest
        {
            Email = email,
            AuthHash = regData.AuthHash,
            AuthSalt = regData.AuthSalt,
            KdfParams = new KdfParamsRequest
            {
                Iterations = regData.KdfParams.Iterations,
                MemorySize = regData.KdfParams.MemorySize,
                Parallelism = regData.KdfParams.Parallelism
            },
            UserPublicKey = regData.KeyEnvelope.PublicKey,
            EncryptedUserPrivateKey = regData.KeyEnvelope.EncryptedPrivateKey,
            EncryptedUserVaultKey = regData.KeyEnvelope.EncryptedVaultKey
        });
        regResponse.EnsureSuccessStatusCode();

        var jwtCookie = regResponse.GetSetCookieValue(HttpCookieKeyEnum.JwtToken.GetKey());
        var accessTokenCookie = regResponse.GetSetCookieValue(HttpCookieKeyEnum.AccessToken.GetKey());
        Assert.NotNull(jwtCookie);
        Assert.NotNull(accessTokenCookie);

        var accessToken = await DbSessionProvider.CurrentSession.Query<UserAccessTokenEntity>()
            .SingleAsync(item => item.Token == accessTokenCookie);

        // Create an expiring JWT token (e.g. expiring in 10 seconds)
        var expirationTime = DateTime.UtcNow.AddSeconds(10);
        var expiringJwtToken = _jwtService.BuildJwt(
            accessToken.User.Id,
            accessToken.Id,
            expirationTime
        );

        await DbSessionProvider.CurrentSession.SaveAsync(new UserJwtTokenEntity
        {
            Token = expiringJwtToken,
            ExpirationTime = expirationTime,
            CreatedAt = DateTime.UtcNow,
            AccessToken = accessToken
        });
        await FlushDbChanges();

        var request = new HttpRequestMessage(HttpMethod.Get, ApiUrl.AuthCheck);
        var jwtCookieName = PrepareCookieName(HttpCookieKeyEnum.JwtToken);
        var accessTokenCookieName = PrepareCookieName(HttpCookieKeyEnum.AccessToken);
        request.Headers.Add(
            "Cookie",
            $"{jwtCookieName}={expiringJwtToken}; " +
            $"{accessTokenCookieName}={accessTokenCookie}"
        );

        var response = await HttpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var refreshedJwtToken = response.GetSetCookieValue(HttpCookieKeyEnum.JwtToken.GetKey());
        Assert.NotNull(refreshedJwtToken);
        Assert.NotEmpty(refreshedJwtToken);
        Assert.NotEqual(expiringJwtToken, refreshedJwtToken);
        Assert.True(_jwtService.IsValidJwt(refreshedJwtToken));
        Assert.Equal(accessToken.User.Id, _jwtService.GetUserId(refreshedJwtToken));
    }

    [Fact]
    public async Task ShouldNotRefreshFreshJwtToken()
    {
        var email = $"fresh_{Guid.NewGuid():N}@example.com";
        var regData = CryptoUtils.PrepareClientRegistration("Password#2026");

        var regResponse = await PostRequestAsAnonymousAsync(ApiUrl.AuthRegister, new RegisterRequest
        {
            Email = email,
            AuthHash = regData.AuthHash,
            AuthSalt = regData.AuthSalt,
            KdfParams = new KdfParamsRequest
            {
                Iterations = regData.KdfParams.Iterations,
                MemorySize = regData.KdfParams.MemorySize,
                Parallelism = regData.KdfParams.Parallelism
            },
            UserPublicKey = regData.KeyEnvelope.PublicKey,
            EncryptedUserPrivateKey = regData.KeyEnvelope.EncryptedPrivateKey,
            EncryptedUserVaultKey = regData.KeyEnvelope.EncryptedVaultKey
        });
        regResponse.EnsureSuccessStatusCode();

        var jwtCookie = regResponse.GetSetCookieValue(HttpCookieKeyEnum.JwtToken.GetKey());
        var accessTokenCookie = regResponse.GetSetCookieValue(HttpCookieKeyEnum.AccessToken.GetKey());
        Assert.NotNull(jwtCookie);
        Assert.NotNull(accessTokenCookie);

        var request = new HttpRequestMessage(HttpMethod.Get, ApiUrl.AuthCheck);
        var jwtCookieName = PrepareCookieName(HttpCookieKeyEnum.JwtToken);
        var accessTokenCookieName = PrepareCookieName(HttpCookieKeyEnum.AccessToken);
        request.Headers.Add(
            "Cookie",
            $"{jwtCookieName}={jwtCookie}; " +
            $"{accessTokenCookieName}={accessTokenCookie}"
        );

        var response = await HttpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        Assert.Null(response.GetSetCookieValue(HttpCookieKeyEnum.JwtToken.GetKey()));
    }

    [Fact]
    public async Task ShouldRejectWhenAccessTokenIsExpiredInDb()
    {
        var email = $"expired_token_{Guid.NewGuid():N}@example.com";
        var regData = CryptoUtils.PrepareClientRegistration("Password#2026");

        var regResponse = await PostRequestAsAnonymousAsync(ApiUrl.AuthRegister, new RegisterRequest
        {
            Email = email,
            AuthHash = regData.AuthHash,
            AuthSalt = regData.AuthSalt,
            KdfParams = new KdfParamsRequest
            {
                Iterations = regData.KdfParams.Iterations,
                MemorySize = regData.KdfParams.MemorySize,
                Parallelism = regData.KdfParams.Parallelism
            },
            UserPublicKey = regData.KeyEnvelope.PublicKey,
            EncryptedUserPrivateKey = regData.KeyEnvelope.EncryptedPrivateKey,
            EncryptedUserVaultKey = regData.KeyEnvelope.EncryptedVaultKey
        });
        regResponse.EnsureSuccessStatusCode();

        var jwtCookie = regResponse.GetSetCookieValue(HttpCookieKeyEnum.JwtToken.GetKey());
        var accessTokenCookie = regResponse.GetSetCookieValue(HttpCookieKeyEnum.AccessToken.GetKey());
        Assert.NotNull(jwtCookie);
        Assert.NotNull(accessTokenCookie);

        // Manually expire access token in DB
        var accessToken = await DbSessionProvider.CurrentSession.Query<UserAccessTokenEntity>()
            .SingleAsync(item => item.Token == accessTokenCookie);
        accessToken.ExpirationTime = DateTime.UtcNow.AddMinutes(-10);
        await DbSessionProvider.CurrentSession.SaveOrUpdateAsync(accessToken);
        await FlushDbChanges(isClearSession: true);

        var request = new HttpRequestMessage(HttpMethod.Get, ApiUrl.AuthCheck);
        var jwtCookieName = PrepareCookieName(HttpCookieKeyEnum.JwtToken);
        var accessTokenCookieName = PrepareCookieName(HttpCookieKeyEnum.AccessToken);
        request.Headers.Add(
            "Cookie",
            $"{jwtCookieName}={jwtCookie}; " +
            $"{accessTokenCookieName}={accessTokenCookie}"
        );

        var response = await HttpClient.SendAsync(request);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}

