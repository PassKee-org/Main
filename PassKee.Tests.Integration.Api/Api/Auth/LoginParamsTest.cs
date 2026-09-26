using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using PassKee.Api.Shared.Constants;
using PassKee.Api.Shared.Dto.RequestsAndResponses.Auth;
using PassKee.Business.Common.Utils;
using PassKee.Tests.Integration.Api.Core;
using Xunit;

namespace PassKee.Tests.Integration.Api.Api.Auth;

public class LoginParamsTest : BaseTest
{
    public LoginParamsTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task ShouldReturnLoginParamsForRegisteredUser()
    {
        var email = $"login_params_{Guid.NewGuid():N}@example.com";
        var regData = CryptoUtils.PrepareClientRegistration("MyPassword#123");

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

        var response = await GetRequestAsAnonymousAsync(ApiUrl.AuthLoginParams, new Dictionary<string, string?>
        {
            { "email", email }
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadFromJsonAsync<LoginParamsResponse>();
        Assert.NotNull(content);
        Assert.Equal(Convert.ToBase64String(regData.AuthSalt), content!.AuthSalt);
        Assert.Equal(regData.KdfParams, content.KdfParams);
    }

    [Fact]
    public async Task ShouldFailWhenEmailDoesNotExist()
    {
        var response = await GetRequestAsAnonymousAsync(ApiUrl.AuthLoginParams, new Dictionary<string, string?>
        {
            { "email", $"unknown_{Guid.NewGuid():N}@example.com" }
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}

