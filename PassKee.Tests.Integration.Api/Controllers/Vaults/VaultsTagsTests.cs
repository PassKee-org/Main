using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using PassKee.Api.Shared.Constants;
using PassKee.Api.Shared.Models.Vaults;
using PassKee.Orm.Entities.Vaults;
using PassKee.Tests.Integration.Api.Core;
using Xunit;

namespace PassKee.Tests.Integration.Api.Controllers.Vaults;

public class VaultsTagsTests : BaseTest
{
    public VaultsTagsTests(ApiCustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Should_Create_And_Update_Tag()
    {
        var (jwtToken, user) = await UserSeeder.CreateAuthorizedAsync();

        // 1. Create Vault
        var createVaultRes = await PostRequestAsync(ApiUrl.Vaults, jwtToken, new CreateVaultRequest
        {
            Name = "Tags Test Vault",
            EncryptedVaultKey = new byte[] { 1, 2, 3 }
        });
        Assert.Equal(HttpStatusCode.OK, createVaultRes.StatusCode);
        var vault = (await createVaultRes.Content.ReadFromJsonAsync<VaultResponse>())!.Vault;

        // 2. Create Tag
        var initialEncryptedName = new byte[] { 10, 20, 30 };
        var createTagRes = await PostRequestAsync(ApiUrl.VaultTags, jwtToken, new CreateTagRequest
        {
            VaultId = vault.Id,
            EncryptedName = initialEncryptedName
        });
        Assert.Equal(HttpStatusCode.OK, createTagRes.StatusCode);
        var tagResponse = await createTagRes.Content.ReadFromJsonAsync<TagResponse>();
        Assert.NotNull(tagResponse?.Tag);
        Assert.Equal(vault.Id, tagResponse!.Tag.VaultId);
        Assert.Equal(initialEncryptedName, tagResponse.Tag.EncryptedName);
        var tagId = tagResponse.Tag.Id;

        // Verify in DB
        DbSessionProvider.CurrentSession.Clear();
        var dbTag = await DbSessionProvider.CurrentSession.GetAsync<TagEntity>(tagId);
        Assert.NotNull(dbTag);
        Assert.Equal(initialEncryptedName, dbTag!.EncryptedName);

        // 3. Update Tag
        var updatedEncryptedName = new byte[] { 40, 50, 60, 70 };
        var updateTagRes = await PutRequestAsync(ApiUrl.VaultTag(tagId), jwtToken, new UpdateTagRequest
        {
            TagId = tagId,
            EncryptedName = updatedEncryptedName
        });
        Assert.Equal(HttpStatusCode.OK, updateTagRes.StatusCode);
        var updatedTagResponse = await updateTagRes.Content.ReadFromJsonAsync<TagResponse>();
        Assert.NotNull(updatedTagResponse?.Tag);
        Assert.Equal(tagId, updatedTagResponse!.Tag.Id);
        Assert.Equal(updatedEncryptedName, updatedTagResponse.Tag.EncryptedName);

        // Verify updated in DB
        DbSessionProvider.CurrentSession.Clear();
        var updatedDbTag = await DbSessionProvider.CurrentSession.GetAsync<TagEntity>(tagId);
        Assert.NotNull(updatedDbTag);
        Assert.Equal(updatedEncryptedName, updatedDbTag!.EncryptedName);
    }

    [Fact]
    public async Task Should_Return_Tags_In_VaultDetails()
    {
        var (jwtToken, user) = await UserSeeder.CreateAuthorizedAsync();

        var createVaultRes = await PostRequestAsync(ApiUrl.Vaults, jwtToken, new CreateVaultRequest
        {
            Name = "Vault With Tags",
            EncryptedVaultKey = new byte[] { 1, 2, 3 }
        });
        var vault = (await createVaultRes.Content.ReadFromJsonAsync<VaultResponse>())!.Vault;

        // Create 2 tags
        var tag1Res = await PostRequestAsync(ApiUrl.VaultTags, jwtToken, new CreateTagRequest
        {
            VaultId = vault.Id,
            EncryptedName = new byte[] { 1, 1 }
        });
        var tag1 = (await tag1Res.Content.ReadFromJsonAsync<TagResponse>())!.Tag;

        var tag2Res = await PostRequestAsync(ApiUrl.VaultTags, jwtToken, new CreateTagRequest
        {
            VaultId = vault.Id,
            EncryptedName = new byte[] { 2, 2 }
        });
        var tag2 = (await tag2Res.Content.ReadFromJsonAsync<TagResponse>())!.Tag;

        // Get Vault Details
        var detailsRes = await GetRequestAsync(ApiUrl.VaultDetails(vault.Id), jwtToken);
        Assert.Equal(HttpStatusCode.OK, detailsRes.StatusCode);
        var details = await detailsRes.Content.ReadFromJsonAsync<VaultDetailsResponse>();
        Assert.NotNull(details);
        Assert.NotNull(details!.Tags);
        Assert.Equal(2, details.Tags.Count);
        Assert.Contains(details.Tags, t => t.Id == tag1.Id);
        Assert.Contains(details.Tags, t => t.Id == tag2.Id);
    }

    [Fact]
    public async Task Should_Enforce_Permissions_On_Tags()
    {
        var (jwtToken1, user1) = await UserSeeder.CreateAuthorizedAsync();
        var (jwtToken2, user2) = await UserSeeder.CreateAuthorizedAsync();

        // User 1 creates vault and tag
        var createVaultRes = await PostRequestAsync(ApiUrl.Vaults, jwtToken1, new CreateVaultRequest
        {
            Name = "User 1 Vault",
            EncryptedVaultKey = new byte[] { 1, 2, 3 }
        });
        var vault1 = (await createVaultRes.Content.ReadFromJsonAsync<VaultResponse>())!.Vault;

        var tag1Res = await PostRequestAsync(ApiUrl.VaultTags, jwtToken1, new CreateTagRequest
        {
            VaultId = vault1.Id,
            EncryptedName = new byte[] { 1, 1 }
        });
        var tag1 = (await tag1Res.Content.ReadFromJsonAsync<TagResponse>())!.Tag;

        // User 2 tries to create tag in User 1's vault -> 400 HasNoAccessException
        var user2CreateTagRes = await PostRequestAsync(ApiUrl.VaultTags, jwtToken2, new CreateTagRequest
        {
            VaultId = vault1.Id,
            EncryptedName = new byte[] { 9, 9 }
        });
        Assert.Equal(HttpStatusCode.BadRequest, user2CreateTagRes.StatusCode);
        var errCreate = await user2CreateTagRes.Content.ReadAsStringAsync();
        Assert.Contains("HasNoAccessException", errCreate);

        // User 2 tries to update User 1's tag -> 400 HasNoAccessException
        var user2UpdateTagRes = await PutRequestAsync(ApiUrl.VaultTag(tag1.Id), jwtToken2, new UpdateTagRequest
        {
            TagId = tag1.Id,
            EncryptedName = new byte[] { 8, 8 }
        });
        Assert.Equal(HttpStatusCode.BadRequest, user2UpdateTagRes.StatusCode);
        var errUpdate = await user2UpdateTagRes.Content.ReadAsStringAsync();
        Assert.Contains("HasNoAccessException", errUpdate);

        // User 1 tries to update non-existent tag -> 400 RecordNotFoundException
        var nonExistentTagId = Guid.NewGuid();
        var updateNonExistentRes = await PutRequestAsync(ApiUrl.VaultTag(nonExistentTagId), jwtToken1, new UpdateTagRequest
        {
            TagId = nonExistentTagId,
            EncryptedName = new byte[] { 7, 7 }
        });
        Assert.Equal(HttpStatusCode.BadRequest, updateNonExistentRes.StatusCode);
        var errNotFound = await updateNonExistentRes.Content.ReadAsStringAsync();
        Assert.Contains("RecordNotFoundException", errNotFound);
    }
}
