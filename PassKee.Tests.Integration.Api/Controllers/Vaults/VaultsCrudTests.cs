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

public class VaultsCrudTests : BaseTest
{
    public VaultsCrudTests(ApiCustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Can_Create_Update_Delete_Directories_And_Credentials()
    {
        // 1. Arrange & Auth via UserSeeder
        var (jwtToken, user) = await UserSeeder.CreateAuthorizedAsync();
        
        // 1.5 Create Vault via API
        var createVaultReq = new CreateVaultRequest
        {
            Name = "API Created Vault",
            EncryptedVaultKey = new byte[] { 10, 20, 30 }
        };
        var createVaultRes = await PostRequestAsync(ApiUrl.Vaults, jwtToken, createVaultReq);
        Assert.Equal(HttpStatusCode.OK, createVaultRes.StatusCode);
        var vaultResponse = await createVaultRes.Content.ReadFromJsonAsync<VaultResponse>();
        Assert.NotNull(vaultResponse?.Vault);
        Assert.Equal("API Created Vault", vaultResponse!.Vault.Name);
        var vaultId = vaultResponse.Vault.Id;

        // 2. Create Directory
        var createDirReq = new CreateDirectoryRequest
        {
            VaultId = vaultId,
            ParentDirectoryId = null,
            EncryptedName = new byte[] { 1, 2, 3 }
        };
        var createDirRes = await PostRequestAsync(ApiUrl.VaultDirectories, jwtToken, createDirReq);
        var err1 = await createDirRes.Content.ReadAsStringAsync();
        Assert.True(createDirRes.StatusCode == HttpStatusCode.OK, err1);
        
        var dirResponse = await createDirRes.Content.ReadFromJsonAsync<DirectoryResponse>();
        Assert.NotNull(dirResponse);
        var dirId = dirResponse!.Directory.Id;

        // 3. Create Credential
        var createCredReq = new CreateCredentialRequest
        {
            VaultId = vaultId,
            DirectoryId = dirId,
            Type = CredentialType.Password,
            EncryptedBody = new byte[] { 4, 5, 6 }
        };
        var createCredRes = await PostRequestAsync(ApiUrl.VaultCredentials, jwtToken, createCredReq);
        var err2 = await createCredRes.Content.ReadAsStringAsync();
        Assert.True(createCredRes.StatusCode == HttpStatusCode.OK, err2);

        var credResponse = await createCredRes.Content.ReadFromJsonAsync<CredentialResponse>();
        Assert.NotNull(credResponse);
        var credId = credResponse!.Credential.Id;

        // 4. Update Credential
        var updateCredReq = new UpdateCredentialRequest
        {
            CredentialId = credId,
            DirectoryId = dirId,
            Type = CredentialType.Login,
            EncryptedBody = new byte[] { 7, 8, 9 }
        };
        var updateCredRes = await PutRequestAsync(ApiUrl.VaultCredential(credId), jwtToken, updateCredReq);
        var err3 = await updateCredRes.Content.ReadAsStringAsync();
        Assert.True(updateCredRes.StatusCode == HttpStatusCode.OK, err3);

        // 5. Delete Credential
        var deleteCredRes = await DeleteRequestAsync(ApiUrl.VaultCredential(credId), jwtToken);
        Assert.Equal(HttpStatusCode.OK, deleteCredRes.StatusCode);

        // 6. Delete Directory
        var deleteDirRes = await DeleteRequestAsync(ApiUrl.VaultDirectory(dirId), jwtToken);
        Assert.Equal(HttpStatusCode.OK, deleteDirRes.StatusCode);
    }

    [Fact]
    public async Task Can_Add_And_Delete_Child_Directories_Recursively()
    {
        // 1. Arrange & Auth via UserSeeder
        var (jwtToken, user) = await UserSeeder.CreateAuthorizedAsync();

        // 2. Create Vault
        var createVaultRes = await PostRequestAsync(ApiUrl.Vaults, jwtToken, new CreateVaultRequest
        {
            Name = "Child Directories Vault",
            EncryptedVaultKey = new byte[] { 1, 2, 3 }
        });
        Assert.Equal(HttpStatusCode.OK, createVaultRes.StatusCode);
        var vaultRes = await createVaultRes.Content.ReadFromJsonAsync<VaultResponse>();
        var vaultId = vaultRes!.Vault.Id;

        // 3. Create Parent Directory (Dir 1)
        var parentDirRes = await PostRequestAsync(ApiUrl.VaultDirectories, jwtToken, new CreateDirectoryRequest
        {
            VaultId = vaultId,
            ParentDirectoryId = null,
            EncryptedName = new byte[] { 10, 20 }
        });
        Assert.Equal(HttpStatusCode.OK, parentDirRes.StatusCode);
        var parentDir = (await parentDirRes.Content.ReadFromJsonAsync<DirectoryResponse>())!.Directory;

        // 4. Create Child Directory (Dir 2 inside Dir 1)
        var childDirRes = await PostRequestAsync(ApiUrl.VaultDirectories, jwtToken, new CreateDirectoryRequest
        {
            VaultId = vaultId,
            ParentDirectoryId = parentDir.Id,
            EncryptedName = new byte[] { 30, 40 }
        });
        Assert.Equal(HttpStatusCode.OK, childDirRes.StatusCode);
        var childDir = (await childDirRes.Content.ReadFromJsonAsync<DirectoryResponse>())!.Directory;
        Assert.Equal(parentDir.Id, childDir.ParentDirectoryId);

        // 5. Create Grandchild Directory (Test 4 inside Dir 2)
        var grandchildDirRes = await PostRequestAsync(ApiUrl.VaultDirectories, jwtToken, new CreateDirectoryRequest
        {
            VaultId = vaultId,
            ParentDirectoryId = childDir.Id,
            EncryptedName = new byte[] { 50, 60 }
        });
        Assert.Equal(HttpStatusCode.OK, grandchildDirRes.StatusCode);
        var grandchildDir = (await grandchildDirRes.Content.ReadFromJsonAsync<DirectoryResponse>())!.Directory;
        Assert.Equal(childDir.Id, grandchildDir.ParentDirectoryId);

        // 6. Create Credential inside child directory and grandchild directory
        var credChildRes = await PostRequestAsync(ApiUrl.VaultCredentials, jwtToken, new CreateCredentialRequest
        {
            VaultId = vaultId,
            DirectoryId = childDir.Id,
            Type = CredentialType.Login,
            EncryptedBody = new byte[] { 70, 80 }
        });
        Assert.Equal(HttpStatusCode.OK, credChildRes.StatusCode);

        var credGrandchildRes = await PostRequestAsync(ApiUrl.VaultCredentials, jwtToken, new CreateCredentialRequest
        {
            VaultId = vaultId,
            DirectoryId = grandchildDir.Id,
            Type = CredentialType.Password,
            EncryptedBody = new byte[] { 90, 100 }
        });
        Assert.Equal(HttpStatusCode.OK, credGrandchildRes.StatusCode);

        // 7. Delete child directory (Dir 2, which has child 'Test 4' AND credentials at both levels)
        var deleteChildRes = await DeleteRequestAsync(ApiUrl.VaultDirectory(childDir.Id), jwtToken);
        var deleteChildErr = await deleteChildRes.Content.ReadAsStringAsync();
        Assert.True(deleteChildRes.StatusCode == HttpStatusCode.OK, deleteChildErr);

        // 8. Verify vault details: Dir 2 and its grandchild and their credentials should be gone, Parent Dir 1 remains
        var detailsRes = await GetRequestAsync(ApiUrl.VaultDetails(vaultId), jwtToken);
        Assert.Equal(HttpStatusCode.OK, detailsRes.StatusCode);
        var details = await detailsRes.Content.ReadFromJsonAsync<VaultDetailsResponse>();
        Assert.NotNull(details);
        Assert.Contains(details.Directories, d => d.Id == parentDir.Id);
        Assert.DoesNotContain(details.Directories, d => d.Id == childDir.Id);
        Assert.DoesNotContain(details.Directories, d => d.Id == grandchildDir.Id);
        Assert.Empty(details.Credentials);

        // 9. Now add another child directory (Dir 3 inside Dir 1) with a credential
        var childDir3Res = await PostRequestAsync(ApiUrl.VaultDirectories, jwtToken, new CreateDirectoryRequest
        {
            VaultId = vaultId,
            ParentDirectoryId = parentDir.Id,
            EncryptedName = new byte[] { 110, 120 }
        });
        Assert.Equal(HttpStatusCode.OK, childDir3Res.StatusCode);
        var childDir3 = (await childDir3Res.Content.ReadFromJsonAsync<DirectoryResponse>())!.Directory;

        var credDir3Res = await PostRequestAsync(ApiUrl.VaultCredentials, jwtToken, new CreateCredentialRequest
        {
            VaultId = vaultId,
            DirectoryId = childDir3.Id,
            Type = CredentialType.SecureNote,
            EncryptedBody = new byte[] { 130, 140 }
        });
        Assert.Equal(HttpStatusCode.OK, credDir3Res.StatusCode);

        // 10. Delete the top-level parent directory (Dir 1)
        var deleteParentRes = await DeleteRequestAsync(ApiUrl.VaultDirectory(parentDir.Id), jwtToken);
        var deleteParentErr = await deleteParentRes.Content.ReadAsStringAsync();
        Assert.True(deleteParentRes.StatusCode == HttpStatusCode.OK, deleteParentErr);

        // 11. Verify vault details: all directories and credentials in the vault are gone
        var finalDetailsRes = await GetRequestAsync(ApiUrl.VaultDetails(vaultId), jwtToken);
        Assert.Equal(HttpStatusCode.OK, finalDetailsRes.StatusCode);
        var finalDetails = await finalDetailsRes.Content.ReadFromJsonAsync<VaultDetailsResponse>();
        Assert.NotNull(finalDetails);
        Assert.Empty(finalDetails.Directories);
        Assert.Empty(finalDetails.Credentials);

        // 12. Verify directly via DB query: soft-deleted (DeletedAt != null)
        var allDirsInDb = await NHibernate.Linq.LinqExtensionMethods.ToListAsync(
            DbSessionProvider.CurrentSession.Query<PassKee.Orm.Entities.Vaults.DirectoryEntity>()
                .Where(d => d.VaultId == vaultId)
        );
        var allCredsInDb = await NHibernate.Linq.LinqExtensionMethods.ToListAsync(
            DbSessionProvider.CurrentSession.Query<PassKee.Orm.Entities.Vaults.CredentialEntity>()
                .Where(c => c.VaultId == vaultId)
        );
        Assert.NotEmpty(allDirsInDb);
        Assert.All(allDirsInDb, d => Assert.NotNull(d.DeletedAt));
        Assert.NotEmpty(allCredsInDb);
        Assert.All(allCredsInDb, c => Assert.NotNull(c.DeletedAt));

        var activeDirs = allDirsInDb.Where(d => d.DeletedAt == null).ToList();
        var activeCreds = allCredsInDb.Where(c => c.DeletedAt == null).ToList();
        Assert.Empty(activeDirs);
        Assert.Empty(activeCreds);
    }
}
