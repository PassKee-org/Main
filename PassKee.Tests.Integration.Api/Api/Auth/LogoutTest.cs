using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using PassKee.Api.Shared.Constants;
using PassKee.Api.Shared.Dto.RequestsAndResponses.Auth;
using PassKee.Business.Common.Constants.Http;
using PassKee.Business.Common.Utils;
using PassKee.Business.Testing.Extensions;
using PassKee.Orm.Dao;
using PassKee.Tests.Integration.Api.Core;
using Xunit;

namespace PassKee.Tests.Integration.Api.Api.Auth;

public class LogoutTest : BaseTest
{
    private readonly IUserAccessTokenDao _accessTokenDao;

    public LogoutTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        _accessTokenDao = ServiceProvider.GetRequiredService<IUserAccessTokenDao>();
    }

    [Fact]
    public async Task ShouldCleanUpAuthCookiesWhenAnonymous()
    {
        var response = await PostRequestAsAnonymousAsync(ApiUrl.AuthLogout, new LogoutRequest());
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        Assert.Equal(string.Empty, response.GetSetCookieValue(HttpCookieKeyEnum.JwtToken.GetKey()));
        Assert.Equal(string.Empty, response.GetSetCookieValue(HttpCookieKeyEnum.AccessToken.GetKey()));
    }

    [Fact]
    public async Task ShouldLogoutSuccessfullyAndInvalidateSession()
    {
        var email = $"logout_user_{Guid.NewGuid():N}@example.com";
        var password = "SecurePassword#2026";
        var regData = CryptoUtils.PrepareClientRegistration(password);

        // 1. Register user
        var regRequest = new RegisterRequest
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
        };
        var regResponse = await PostRequestAsAnonymousAsync(ApiUrl.AuthRegister, regRequest);
        Assert.Equal(HttpStatusCode.OK, regResponse.StatusCode);

        var regContent = await regResponse.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(regContent);
        Assert.NotNull(regContent!.UserPublicKey);

        var jwtCookieName = PrepareCookieName(HttpCookieKeyEnum.JwtToken);
        var accessTokenCookieName = PrepareCookieName(HttpCookieKeyEnum.AccessToken);

        var jwtCookie = regResponse.GetSetCookieValue(HttpCookieKeyEnum.JwtToken.GetKey());
        Assert.NotNull(jwtCookie);
        Assert.NotEmpty(jwtCookie);

        var accessTokenCookie = regResponse.GetSetCookieValue(HttpCookieKeyEnum.AccessToken.GetKey());
        Assert.NotNull(accessTokenCookie);
        Assert.NotEmpty(accessTokenCookie);

        // 2. Verify access token entity exists in DB
        var tokenEntity = await _accessTokenDao.GetByToken(accessTokenCookie);
        Assert.NotNull(tokenEntity);

        // 3. Verify user can access protected endpoint
        var checkRequest = new HttpRequestMessage(HttpMethod.Get, ApiUrl.AuthCheck);
        checkRequest.Headers.Add("Cookie", $"{jwtCookieName}={jwtCookie}; {accessTokenCookieName}={accessTokenCookie}");
        var checkResponse = await HttpClient.SendAsync(checkRequest);
        Assert.Equal(HttpStatusCode.OK, checkResponse.StatusCode);

        // 4. Logout with cookies
        var logoutRequest = new HttpRequestMessage(HttpMethod.Post, ApiUrl.AuthLogout);
        logoutRequest.Content = JsonContent.Create(new LogoutRequest());
        logoutRequest.Headers.Add("Cookie", $"{jwtCookieName}={jwtCookie}; {accessTokenCookieName}={accessTokenCookie}");
        var logoutResponse = await HttpClient.SendAsync(logoutRequest);
        Assert.Equal(HttpStatusCode.OK, logoutResponse.StatusCode);

        // Verify cookies cleared
        Assert.Equal(string.Empty, logoutResponse.GetSetCookieValue(jwtCookieName));
        Assert.Equal(string.Empty, logoutResponse.GetSetCookieValue(accessTokenCookieName));

        // 5. Verify access token is deleted from DB
        await FlushDbChanges(isClearSession: true);
        var deletedTokenEntity = await _accessTokenDao.GetByToken(accessTokenCookie);
        Assert.Null(deletedTokenEntity);

        // 6. Verify subsequent requests with old credentials fail
        var oldCheckRequest = new HttpRequestMessage(HttpMethod.Get, ApiUrl.AuthCheck);
        oldCheckRequest.Headers.Add("Cookie", $"{jwtCookieName}={jwtCookie}; {accessTokenCookieName}={accessTokenCookie}");
        var oldCheckResponse = await HttpClient.SendAsync(oldCheckRequest);
        Assert.Equal(HttpStatusCode.Unauthorized, oldCheckResponse.StatusCode);
    }
}
