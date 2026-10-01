using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using PassKee.Api.Shared.Constants;
using PassKee.Api.Shared.Models.Vaults;
using PassKee.Business.Common.Constants;
using PassKee.Tests.Integration.Api.Core;
using Xunit;

namespace PassKee.Tests.Integration.Api.Controllers.Vaults;

public class VaultsPermissionsTests : BaseTest
{
    public VaultsPermissionsTests(ApiCustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task OtherUser_CannotAccess_UserResources()
    {
        // 1. User 1 creates vault, directory, credential
        var (jwtToken1, user1) = await UserSeeder.CreateAuthorizedAsync();
        var (jwtToken2, user2) = await UserSeeder.CreateAuthorizedAsync();

        var createVaultRes = await PostRequestAsync(ApiUrl.Vaults, jwtToken1, new CreateVaultRequest
        {
            Name = "User 1 Private Vault",
            EncryptedVaultKey = new byte[] { 1, 2, 3 }
        });
        Assert.Equal(HttpStatusCode.OK, createVaultRes.StatusCode);
        var vault = (await createVaultRes.Content.ReadFromJsonAsync<VaultResponse>())!.Vault;

        var createDirRes = await PostRequestAsync(ApiUrl.VaultDirectories, jwtToken1, new CreateDirectoryRequest
        {
            VaultId = vault.Id,
            ParentDirectoryId = null,
            EncryptedName = new byte[] { 10, 20 }
        });
        Assert.Equal(HttpStatusCode.OK, createDirRes.StatusCode);
        var dir = (await createDirRes.Content.ReadFromJsonAsync<DirectoryResponse>())!.Directory;

        var createCredRes = await PostRequestAsync(ApiUrl.VaultCredentials, jwtToken1, new CreateCredentialRequest
        {
            VaultId = vault.Id,
            DirectoryId = dir.Id,
            Type = CredentialType.Login,
            EncryptedBody = new byte[] { 30, 40 }
        });
        Assert.Equal(HttpStatusCode.OK, createCredRes.StatusCode);
        var cred = (await createCredRes.Content.ReadFromJsonAsync<CredentialResponse>())!.Credential;

        // 2. User 2 tries to GET User 1's vault details -> 400 HasNoAccessException
        var getVaultRes = await GetRequestAsync(ApiUrl.VaultDetails(vault.Id), jwtToken2);
        Assert.Equal(HttpStatusCode.BadRequest, getVaultRes.StatusCode);
        var getVaultBody = await getVaultRes.Content.ReadAsStringAsync();
        Assert.Contains("HasNoAccessException", getVaultBody);

        // 3. User 2 tries to create directory in User 1's vault -> 400 HasNoAccessException
        var user2CreateDirRes = await PostRequestAsync(ApiUrl.VaultDirectories, jwtToken2, new CreateDirectoryRequest
        {
            VaultId = vault.Id,
            ParentDirectoryId = null,
            EncryptedName = new byte[] { 99, 99 }
        });
        Assert.Equal(HttpStatusCode.BadRequest, user2CreateDirRes.StatusCode);
        var user2CreateDirBody = await user2CreateDirRes.Content.ReadAsStringAsync();
        Assert.Contains("HasNoAccessException", user2CreateDirBody);

        // 4. User 2 tries to update User 1's directory -> 400 HasNoAccessException
        var user2UpdateDirRes = await PutRequestAsync(ApiUrl.VaultDirectory(dir.Id), jwtToken2, new UpdateDirectoryRequest
        {
            DirectoryId = dir.Id,
            ParentDirectoryId = null,
            EncryptedName = new byte[] { 88, 88 }
        });
        Assert.Equal(HttpStatusCode.BadRequest, user2UpdateDirRes.StatusCode);
        var user2UpdateDirBody = await user2UpdateDirRes.Content.ReadAsStringAsync();
        Assert.Contains("HasNoAccessException", user2UpdateDirBody);

        // 5. User 2 tries to delete User 1's directory -> 400 HasNoAccessException
        var user2DeleteDirRes = await DeleteRequestAsync(ApiUrl.VaultDirectory(dir.Id), jwtToken2);
        Assert.Equal(HttpStatusCode.BadRequest, user2DeleteDirRes.StatusCode);
        var user2DeleteDirBody = await user2DeleteDirRes.Content.ReadAsStringAsync();
        Assert.Contains("HasNoAccessException", user2DeleteDirBody);

        // 6. User 2 tries to create credential in User 1's vault -> 400 HasNoAccessException
        var user2CreateCredRes = await PostRequestAsync(ApiUrl.VaultCredentials, jwtToken2, new CreateCredentialRequest
        {
            VaultId = vault.Id,
            DirectoryId = null,
            Type = CredentialType.Password,
            EncryptedBody = new byte[] { 77, 77 }
        });
        Assert.Equal(HttpStatusCode.BadRequest, user2CreateCredRes.StatusCode);
        var user2CreateCredBody = await user2CreateCredRes.Content.ReadAsStringAsync();
        Assert.Contains("HasNoAccessException", user2CreateCredBody);

        // 7. User 2 tries to update User 1's credential -> 400 HasNoAccessException
        var user2UpdateCredRes = await PutRequestAsync(ApiUrl.VaultCredential(cred.Id), jwtToken2, new UpdateCredentialRequest
        {
            CredentialId = cred.Id,
            DirectoryId = null,
            Type = CredentialType.Card,
            EncryptedBody = new byte[] { 66, 66 }
        });
        Assert.Equal(HttpStatusCode.BadRequest, user2UpdateCredRes.StatusCode);
        var user2UpdateCredBody = await user2UpdateCredRes.Content.ReadAsStringAsync();
        Assert.Contains("HasNoAccessException", user2UpdateCredBody);

        // 8. User 2 tries to delete User 1's credential -> 400 HasNoAccessException
        var user2DeleteCredRes = await DeleteRequestAsync(ApiUrl.VaultCredential(cred.Id), jwtToken2);
        Assert.Equal(HttpStatusCode.BadRequest, user2DeleteCredRes.StatusCode);
        var user2DeleteCredBody = await user2DeleteCredRes.Content.ReadAsStringAsync();
        Assert.Contains("HasNoAccessException", user2DeleteCredBody);
    }

    [Fact]
    public async Task NonExistentEntities_ReturnRecordNotFound()
    {
        var (jwtToken, _) = await UserSeeder.CreateAuthorizedAsync();
        var randomId = Guid.NewGuid();

        // 1. GET non-existent vault -> 400 RecordNotFoundException
        var getVaultRes = await GetRequestAsync(ApiUrl.VaultDetails(randomId), jwtToken);
        Assert.Equal(HttpStatusCode.BadRequest, getVaultRes.StatusCode);
        var getVaultBody = await getVaultRes.Content.ReadAsStringAsync();
        Assert.Contains("RecordNotFoundException", getVaultBody);

        // 2. DELETE non-existent directory -> 400 RecordNotFoundException
        var deleteDirRes = await DeleteRequestAsync(ApiUrl.VaultDirectory(randomId), jwtToken);
        Assert.Equal(HttpStatusCode.BadRequest, deleteDirRes.StatusCode);
        var deleteDirBody = await deleteDirRes.Content.ReadAsStringAsync();
        Assert.Contains("RecordNotFoundException", deleteDirBody);

        // 3. DELETE non-existent credential -> 400 RecordNotFoundException
        var deleteCredRes = await DeleteRequestAsync(ApiUrl.VaultCredential(randomId), jwtToken);
        Assert.Equal(HttpStatusCode.BadRequest, deleteCredRes.StatusCode);
        var deleteCredBody = await deleteCredRes.Content.ReadAsStringAsync();
        Assert.Contains("RecordNotFoundException", deleteCredBody);
    }
}
