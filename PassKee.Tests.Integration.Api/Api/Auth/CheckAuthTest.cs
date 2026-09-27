using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using PassKee.Api.Shared.Constants;
using PassKee.Api.Shared.Dto.RequestsAndResponses.Auth;
using PassKee.Business.Common.Constants.Http;
using PassKee.Business.Common.Utils;
using PassKee.Business.Testing.Extensions;
using PassKee.Tests.Integration.Api.Core;
using Xunit;

namespace PassKee.Tests.Integration.Api.Api.Auth;

public class CheckAuthTest : BaseTest
{
    public CheckAuthTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task ShouldAuthorizeSuccessfullyWithCookie()
    {
        var email = $"check_auth_{Guid.NewGuid():N}@example.com";
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

        // Extract cookie
        var cookieInfo = regResponse.GetSetCookie(HttpCookieKeyEnum.JwtToken.GetKey());
        Assert.NotNull(cookieInfo);
        var (cookieName, jwtCookie) = cookieInfo.Value;
        Assert.NotEmpty(jwtCookie);

        // 2. Check auth with cookie
        var checkResponse = await GetRequestWithCookieAsync(ApiUrl.AuthCheck, cookieName, jwtCookie);
        Assert.Equal(HttpStatusCode.OK, checkResponse.StatusCode);
    }

    [Fact]
    public async Task ShouldAuthorizeSuccessfullyWithBearerToken()
    {
        var email = $"check_bearer_{Guid.NewGuid():N}@example.com";
        var password = "SecurePassword#2026";
        var regData = CryptoUtils.PrepareClientRegistration(password);

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

        var content = await regResponse.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(content);
        Assert.NotNull(content!.UserPublicKey);

        // Check auth with bearer token (extracted from auth cookie)
        var jwtToken = regResponse.GetSetCookieValue(HttpCookieKeyEnum.JwtToken.GetKey());
        Assert.NotNull(jwtToken);
        Assert.NotEmpty(jwtToken);

        var checkResponse = await GetRequestAsync(ApiUrl.AuthCheck, jwtToken);
        Assert.Equal(HttpStatusCode.OK, checkResponse.StatusCode);
    }

    [Fact]
    public async Task ShouldFailWhenAnonymous()
    {
        var checkResponse = await GetRequestAsAnonymousAsync(ApiUrl.AuthCheck);
        Assert.Equal(HttpStatusCode.Unauthorized, checkResponse.StatusCode);
    }

    [Fact]
    public async Task ShouldFailWithInvalidCookie()
    {
        var checkResponse = await GetRequestWithCookieAsync(ApiUrl.AuthCheck, HttpCookieKeyEnum.JwtToken.GetKey(), "invalid_garbage_token");
        Assert.Equal(HttpStatusCode.Unauthorized, checkResponse.StatusCode);
    }
}
