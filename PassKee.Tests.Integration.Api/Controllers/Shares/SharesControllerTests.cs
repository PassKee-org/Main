using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using PassKee.Api.Shared.Constants;
using PassKee.Api.Shared.Models.Shares;
using PassKee.Business.Common.Constants;
using PassKee.Orm.Dao.Vaults;
using PassKee.Tests.Integration.Api.Core;
using Xunit;

namespace PassKee.Tests.Integration.Api.Controllers.Shares;

public class SharesControllerTests : BaseTest
{
    public SharesControllerTests(ApiCustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Can_Create_Credential_Share()
    {
        var (jwtToken, user) = await UserSeeder.CreateAuthorizedAsync();

        var request = new CreateCredentialShareRequest
        {
            Type = CredentialType.Login,
            EncryptedBody = new byte[] { 1, 2, 3, 4, 5 },
            ExpiresInSeconds = 3600,
            MaxViews = null
        };

        var response = await PostRequestAsync(ApiUrl.Shares, jwtToken, request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadFromJsonAsync<CreateCredentialShareResponse>();
        Assert.NotNull(content);
        Assert.False(string.IsNullOrWhiteSpace(content!.Token));
        Assert.NotNull(content.ExpiresAt);
        Assert.Null(content.MaxViews);

        // Verify entity in DB
        var shareDao = ServiceProvider.GetRequiredService<ICredentialShareDao>();
        var entity = await shareDao.GetByTokenAsync(content.Token);
        Assert.NotNull(entity);
        Assert.Equal(user.Id, entity!.UserId);
        Assert.Equal(CredentialType.Login, entity.Type);
        Assert.Equal(new byte[] { 1, 2, 3, 4, 5 }, entity.EncryptedBody);
        Assert.Equal(0, entity.ViewCount);
    }

    [Fact]
    public async Task Can_Get_Credential_Share_Without_Authorization()
    {
        var (jwtToken, _) = await UserSeeder.CreateAuthorizedAsync();

        var encryptedData = new byte[] { 10, 20, 30, 40 };
        var createRequest = new CreateCredentialShareRequest
        {
            Type = CredentialType.Password,
            EncryptedBody = encryptedData,
            ExpiresInSeconds = 86400,
            MaxViews = null
        };

        var createRes = await PostRequestAsync(ApiUrl.Shares, jwtToken, createRequest);
        Assert.Equal(HttpStatusCode.OK, createRes.StatusCode);
        var created = await createRes.Content.ReadFromJsonAsync<CreateCredentialShareResponse>();

        // Anonymous GET request without any bearer token
        var getRes = await GetRequestAsAnonymousAsync(ApiUrl.Share(created!.Token));
        Assert.Equal(HttpStatusCode.OK, getRes.StatusCode);

        var sharedResponse = await getRes.Content.ReadFromJsonAsync<CredentialShareResponse>();
        Assert.NotNull(sharedResponse);
        Assert.Equal(CredentialType.Password, sharedResponse!.Type);
        Assert.Equal(encryptedData, sharedResponse.EncryptedBody);
    }

    [Fact]
    public async Task Credential_Share_With_Max_Views_1_Is_Deleted_After_Viewing()
    {
        var (jwtToken, _) = await UserSeeder.CreateAuthorizedAsync();

        var createRequest = new CreateCredentialShareRequest
        {
            Type = CredentialType.Card,
            EncryptedBody = new byte[] { 99, 88, 77 },
            ExpiresInSeconds = 3600,
            MaxViews = 1
        };

        var createRes = await PostRequestAsync(ApiUrl.Shares, jwtToken, createRequest);
        Assert.Equal(HttpStatusCode.OK, createRes.StatusCode);
        var created = await createRes.Content.ReadFromJsonAsync<CreateCredentialShareResponse>();

        // 1st view (anonymous) -> should succeed
        var firstGetRes = await GetRequestAsAnonymousAsync(ApiUrl.Share(created!.Token));
        Assert.Equal(HttpStatusCode.OK, firstGetRes.StatusCode);
        var payload = await firstGetRes.Content.ReadFromJsonAsync<CredentialShareResponse>();
        Assert.NotNull(payload);
        Assert.Equal(new byte[] { 99, 88, 77 }, payload!.EncryptedBody);

        // 2nd view (anonymous) -> must fail because max_views was reached and entry was deleted
        var secondGetRes = await GetRequestAsAnonymousAsync(ApiUrl.Share(created.Token));
        Assert.Equal(HttpStatusCode.BadRequest, secondGetRes.StatusCode);

        // Verify entity is permanently deleted from DB
        using var scope = _factory.Services.CreateScope();
        var shareDao = scope.ServiceProvider.GetRequiredService<ICredentialShareDao>();
        var entity = await shareDao.GetByTokenAsync(created.Token);
        Assert.Null(entity);
    }

    [Fact]
    public async Task Credential_Share_With_Max_Views_Null_Allows_Multiple_Views()
    {
        var (jwtToken, _) = await UserSeeder.CreateAuthorizedAsync();

        var createRequest = new CreateCredentialShareRequest
        {
            Type = CredentialType.SecureNote,
            EncryptedBody = new byte[] { 42 },
            ExpiresInSeconds = 3600,
            MaxViews = null
        };

        var createRes = await PostRequestAsync(ApiUrl.Shares, jwtToken, createRequest);
        var created = await createRes.Content.ReadFromJsonAsync<CreateCredentialShareResponse>();

        // 1st view
        var res1 = await GetRequestAsAnonymousAsync(ApiUrl.Share(created!.Token));
        Assert.Equal(HttpStatusCode.OK, res1.StatusCode);

        // 2nd view
        var res2 = await GetRequestAsAnonymousAsync(ApiUrl.Share(created.Token));
        Assert.Equal(HttpStatusCode.OK, res2.StatusCode);

        // 3rd view
        var res3 = await GetRequestAsAnonymousAsync(ApiUrl.Share(created.Token));
        Assert.Equal(HttpStatusCode.OK, res3.StatusCode);

        // Verify entity is still present in DB with incremented ViewCount
        using var scope = _factory.Services.CreateScope();
        var shareDao = scope.ServiceProvider.GetRequiredService<ICredentialShareDao>();
        var entity = await shareDao.GetByTokenAsync(created.Token);
        Assert.NotNull(entity);
        Assert.Equal(3, entity!.ViewCount);
    }

    [Fact]
    public async Task Credential_Share_Expired_Cannot_Be_Viewed()
    {
        var (jwtToken, _) = await UserSeeder.CreateAuthorizedAsync();

        var createRequest = new CreateCredentialShareRequest
        {
            Type = CredentialType.Login,
            EncryptedBody = new byte[] { 1, 2, 3 },
            ExpiresInSeconds = -10 // Already expired in the past
        };

        var createRes = await PostRequestAsync(ApiUrl.Shares, jwtToken, createRequest);
        var created = await createRes.Content.ReadFromJsonAsync<CreateCredentialShareResponse>();

        var getRes = await GetRequestAsAnonymousAsync(ApiUrl.Share(created!.Token));
        Assert.Equal(HttpStatusCode.BadRequest, getRes.StatusCode);

        // Verify deleted from DB
        using var scope = _factory.Services.CreateScope();
        var shareDao = scope.ServiceProvider.GetRequiredService<ICredentialShareDao>();
        var entity = await shareDao.GetByTokenAsync(created.Token);
        Assert.Null(entity);
    }

    [Fact]
    public async Task NonExistent_Token_Returns_BadRequest()
    {
        var getRes = await GetRequestAsAnonymousAsync(ApiUrl.Share("non-existent-token-abc"));
        Assert.Equal(HttpStatusCode.BadRequest, getRes.StatusCode);
    }
}
