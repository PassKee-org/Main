using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using PassKee.Api.Shared.Constants;
using PassKee.Api.Shared.Dto.RequestsAndResponses.Auth;
using PassKee.Business.Common.Utils;
using PassKee.Tests.Integration.Api.Core;
using Xunit;

namespace PassKee.Tests.Integration.Api.Api.Auth;

public class LoginTest : BaseTest
{
    public LoginTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task ShouldLoginSuccessfully()
    {
        var email = $"login_user_{Guid.NewGuid():N}@example.com";
        var password = "SecureMasterPassword#2026";
        var regData = CryptoUtils.PrepareClientRegistration(password);

        // 1. Register user
        var regRequest = new RegisterRequest
        {
            Email = email,
            AuthHash = regData.AuthHash,
            AuthSalt = regData.AuthSalt,
            KdfParams = regData.KdfParams,
            UserPublicKey = regData.KeyEnvelope.PublicKey,
            EncryptedUserPrivateKey = regData.KeyEnvelope.EncryptedPrivateKey,
            EncryptedUserVaultKey = regData.KeyEnvelope.EncryptedVaultKey
        };
        var regResponse = await PostRequestAsAnonymousAsync(ApiUrl.AuthRegister, regRequest);
        Assert.Equal(HttpStatusCode.OK, regResponse.StatusCode);

        // 2. Client derives auth hash on login
        var loginMasterKey = CryptoUtils.DeriveMasterKey(password, regData.SecretKey, regData.AuthSalt);
        var loginAuthHash = CryptoUtils.ComputeAuthHash(loginMasterKey);

        // 3. Login
        var loginResponse = await PostRequestAsAnonymousAsync(ApiUrl.AuthLogin, new LoginRequest
        {
            Email = email,
            AuthHash = loginAuthHash
        });

        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
        var loginContent = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(loginContent);
        Assert.NotEmpty(loginContent!.AccessToken);
        Assert.Equal(Convert.ToBase64String(regData.KeyEnvelope.PublicKey), loginContent.UserPublicKey);
    }

    [Fact]
    public async Task ShouldFailWhenAuthHashIsIncorrect()
    {
        var email = $"login_fail_{Guid.NewGuid():N}@example.com";
        var regData = CryptoUtils.PrepareClientRegistration("ValidPassword#1");

        await PostRequestAsAnonymousAsync(ApiUrl.AuthRegister, new RegisterRequest
        {
            Email = email,
            AuthHash = regData.AuthHash,
            AuthSalt = regData.AuthSalt,
            KdfParams = regData.KdfParams,
            UserPublicKey = regData.KeyEnvelope.PublicKey,
            EncryptedUserPrivateKey = regData.KeyEnvelope.EncryptedPrivateKey,
            EncryptedUserVaultKey = regData.KeyEnvelope.EncryptedVaultKey
        });

        // Try to login with wrong auth hash
        var wrongAuthHash = CryptoUtils.GenerateRandomBytes(32);
        var loginResponse = await PostRequestAsAnonymousAsync(ApiUrl.AuthLogin, new LoginRequest
        {
            Email = email,
            AuthHash = wrongAuthHash
        });

        Assert.Equal(HttpStatusCode.Unauthorized, loginResponse.StatusCode);
    }

    [Fact]
    public async Task ShouldFailWhenUserDoesNotExist()
    {
        var loginResponse = await PostRequestAsAnonymousAsync(ApiUrl.AuthLogin, new LoginRequest
        {
            Email = $"nonexistent_{Guid.NewGuid():N}@example.com",
            AuthHash = CryptoUtils.GenerateRandomBytes(32)
        });

        Assert.Equal(HttpStatusCode.Unauthorized, loginResponse.StatusCode);
    }
}
