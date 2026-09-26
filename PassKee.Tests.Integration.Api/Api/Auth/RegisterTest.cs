using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using PassKee.Api.Shared.Constants;
using PassKee.Api.Shared.Dto.RequestsAndResponses.Auth;
using PassKee.Business.Common.Utils;
using PassKee.Orm.Dao;
using PassKee.Tests.Integration.Api.Core;
using Xunit;

namespace PassKee.Tests.Integration.Api.Api.Auth;

public class RegisterTest : BaseTest
{
    private readonly IUserDao _userDao;

    public RegisterTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        _userDao = ServiceProvider.GetRequiredService<IUserDao>();
    }

    [Fact]
    public async Task ShouldRegisterUserSuccessfully()
    {
        var email = $"new_user_{Guid.NewGuid():N}@example.com";
        var regData = CryptoUtils.PrepareClientRegistration("MasterP@ssword123!");

        var request = new RegisterRequest
        {
            Email = email,
            AuthHash = regData.AuthHash,
            AuthSalt = regData.AuthSalt,
            KdfParams = regData.KdfParams,
            UserPublicKey = regData.KeyEnvelope.PublicKey,
            EncryptedUserPrivateKey = regData.KeyEnvelope.EncryptedPrivateKey,
            EncryptedUserVaultKey = regData.KeyEnvelope.EncryptedVaultKey
        };

        var response = await PostRequestAsAnonymousAsync(ApiUrl.AuthRegister, request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(content);
        Assert.NotEmpty(content!.AccessToken);
        Assert.Equal(Convert.ToBase64String(regData.KeyEnvelope.PublicKey), content.UserPublicKey);
        Assert.Equal(Convert.ToBase64String(regData.KeyEnvelope.EncryptedPrivateKey), content.EncryptedUserPrivateKey);
        Assert.Equal(Convert.ToBase64String(regData.KeyEnvelope.EncryptedVaultKey), content.EncryptedUserVaultKey);

        // Verify entity persisted in database with raw binary keys
        var userInDb = await _userDao.GetByEmail(email);
        Assert.NotNull(userInDb);
        Assert.Equal(email, userInDb!.Email);
        Assert.Equal(regData.AuthSalt, userInDb.AuthSalt);
        Assert.NotNull(userInDb.ServerHash);
        Assert.Equal(regData.KeyEnvelope.PublicKey, userInDb.UserPublicKey);
        Assert.Equal(regData.KeyEnvelope.EncryptedPrivateKey, userInDb.EncryptedUserPrivateKey);
        Assert.Equal(regData.KeyEnvelope.EncryptedVaultKey, userInDb.EncryptedUserVaultKey);
    }

    [Fact]
    public async Task ShouldFailWhenRegisteringExistingEmail()
    {
        var email = $"duplicate_{Guid.NewGuid():N}@example.com";
        var regData = CryptoUtils.PrepareClientRegistration("MasterP@ssword123!");

        var request = new RegisterRequest
        {
            Email = email,
            AuthHash = regData.AuthHash,
            AuthSalt = regData.AuthSalt,
            KdfParams = regData.KdfParams,
            UserPublicKey = regData.KeyEnvelope.PublicKey,
            EncryptedUserPrivateKey = regData.KeyEnvelope.EncryptedPrivateKey,
            EncryptedUserVaultKey = regData.KeyEnvelope.EncryptedVaultKey
        };

        var firstResponse = await PostRequestAsAnonymousAsync(ApiUrl.AuthRegister, request);
        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);

        var secondResponse = await PostRequestAsAnonymousAsync(ApiUrl.AuthRegister, request);
        Assert.Equal(HttpStatusCode.BadRequest, secondResponse.StatusCode);
    }
}

