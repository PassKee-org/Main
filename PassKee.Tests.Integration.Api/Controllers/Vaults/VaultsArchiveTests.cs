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

public class VaultsArchiveTests : BaseTest
{
    public VaultsArchiveTests(ApiCustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Can_Archive_Credential_And_Verify_VaultDetails_And_ArchivedEndpoint()
    {
        // 1. Arrange & Auth
        var (jwtToken, user) = await UserSeeder.CreateAuthorizedAsync();

        // 2. Create Vault
        var createVaultReq = new CreateVaultRequest
        {
            Name = "Archive Test Vault",
            EncryptedVaultKey = new byte[] { 10, 20, 30 }
        };
        var createVaultRes = await PostRequestAsync(ApiUrl.Vaults, jwtToken, createVaultReq);
        Assert.Equal(HttpStatusCode.OK, createVaultRes.StatusCode);
        var vaultResponse = await createVaultRes.Content.ReadFromJsonAsync<VaultResponse>();
        Assert.NotNull(vaultResponse?.Vault);
        var vaultId = vaultResponse!.Vault.Id;

        // 3. Create Directory
        var createDirReq = new CreateDirectoryRequest
        {
            VaultId = vaultId,
            ParentDirectoryId = null,
            EncryptedName = new byte[] { 1, 2, 3 }
        };
        var createDirRes = await PostRequestAsync(ApiUrl.VaultDirectories, jwtToken, createDirReq);
        Assert.Equal(HttpStatusCode.OK, createDirRes.StatusCode);
        var dirResponse = await createDirRes.Content.ReadFromJsonAsync<DirectoryResponse>();
        Assert.NotNull(dirResponse);
        var dirId = dirResponse!.Directory.Id;

        // 4. Create two credentials in the directory
        var createCred1Req = new CreateCredentialRequest
        {
            VaultId = vaultId,
            DirectoryId = dirId,
            Type = CredentialType.Password,
            EncryptedBody = new byte[] { 100, 101, 102 }
        };
        var createCred1Res = await PostRequestAsync(ApiUrl.VaultCredentials, jwtToken, createCred1Req);
        Assert.Equal(HttpStatusCode.OK, createCred1Res.StatusCode);
        var cred1Response = await createCred1Res.Content.ReadFromJsonAsync<CredentialResponse>();
        Assert.NotNull(cred1Response);
        var cred1Id = cred1Response!.Credential.Id;

        var createCred2Req = new CreateCredentialRequest
        {
            VaultId = vaultId,
            DirectoryId = dirId,
            Type = CredentialType.Login,
            EncryptedBody = new byte[] { 200, 201, 202 }
        };
        var createCred2Res = await PostRequestAsync(ApiUrl.VaultCredentials, jwtToken, createCred2Req);
        Assert.Equal(HttpStatusCode.OK, createCred2Res.StatusCode);
        var cred2Response = await createCred2Res.Content.ReadFromJsonAsync<CredentialResponse>();
        Assert.NotNull(cred2Response);
        var cred2Id = cred2Response!.Credential.Id;

        // 5. Archive cred1
        var archiveRes = await PostRequestAsync(ApiUrl.VaultCredentialArchive(cred1Id), jwtToken, new { });
        var errArchive = await archiveRes.Content.ReadAsStringAsync();
        Assert.True(archiveRes.StatusCode == HttpStatusCode.OK, errArchive);

        var archiveResponse = await archiveRes.Content.ReadFromJsonAsync<CredentialResponse>();
        Assert.NotNull(archiveResponse?.Credential);
        Assert.Equal(cred1Id, archiveResponse!.Credential.Id);
        Assert.NotNull(archiveResponse.Credential.ArchivedAt);
        Assert.Equal(dirId, archiveResponse.Credential.DirectoryId);

        // 6. Verify GetVaultDetails does NOT return archived cred1 (Requirement 6)
        var vaultDetailsRes = await GetRequestAsync(ApiUrl.VaultDetails(vaultId), jwtToken);
        Assert.Equal(HttpStatusCode.OK, vaultDetailsRes.StatusCode);
        var vaultDetails = await vaultDetailsRes.Content.ReadFromJsonAsync<VaultDetailsResponse>();
        Assert.NotNull(vaultDetails);
        Assert.DoesNotContain(vaultDetails!.Credentials, c => c.Id == cred1Id);
        Assert.Contains(vaultDetails.Credentials, c => c.Id == cred2Id);

        // 7. Verify GetArchivedCredentials returns cred1 (Requirement 7)
        var archivedRes = await GetRequestAsync(ApiUrl.VaultArchivedCredentials(vaultId), jwtToken);
        Assert.Equal(HttpStatusCode.OK, archivedRes.StatusCode);
        var archivedList = await archivedRes.Content.ReadFromJsonAsync<ArchivedCredentialsResponse>();
        Assert.NotNull(archivedList);
        Assert.Single(archivedList!.Credentials);
        var archivedCred1 = archivedList.Credentials[0];
        Assert.Equal(cred1Id, archivedCred1.Id);
        Assert.NotNull(archivedCred1.ArchivedAt);
        Assert.Equal(dirId, archivedCred1.DirectoryId);

        // 8. Delete directory and verify cred1 is NOT deleted and retains its DirectoryId (Requirement 4)
        var deleteDirRes = await DeleteRequestAsync(ApiUrl.VaultDirectory(dirId), jwtToken);
        Assert.Equal(HttpStatusCode.OK, deleteDirRes.StatusCode);

        var archivedAfterDirDeleteRes = await GetRequestAsync(ApiUrl.VaultArchivedCredentials(vaultId), jwtToken);
        Assert.Equal(HttpStatusCode.OK, archivedAfterDirDeleteRes.StatusCode);
        var archivedAfterDirDelete = await archivedAfterDirDeleteRes.Content.ReadFromJsonAsync<ArchivedCredentialsResponse>();
        Assert.NotNull(archivedAfterDirDelete);
        Assert.Single(archivedAfterDirDelete!.Credentials);
        Assert.Equal(cred1Id, archivedAfterDirDelete.Credentials[0].Id);
        Assert.Equal(dirId, archivedAfterDirDelete.Credentials[0].DirectoryId);

        // 9. Delete archived credential
        var deleteCredRes = await DeleteRequestAsync(ApiUrl.VaultCredential(cred1Id), jwtToken);
        Assert.Equal(HttpStatusCode.OK, deleteCredRes.StatusCode);

        var finalArchivedRes = await GetRequestAsync(ApiUrl.VaultArchivedCredentials(vaultId), jwtToken);
        Assert.Equal(HttpStatusCode.OK, finalArchivedRes.StatusCode);
        var finalArchived = await finalArchivedRes.Content.ReadFromJsonAsync<ArchivedCredentialsResponse>();
        Assert.NotNull(finalArchived);
        Assert.Empty(finalArchived!.Credentials);
    }

    [Fact]
    public async Task Non_Owner_Cannot_Archive_Or_Get_Archived_Credentials()
    {
        // 1. User 1 creates vault and credential
        var (jwtToken1, user1) = await UserSeeder.CreateAuthorizedAsync();
        var (jwtToken2, user2) = await UserSeeder.CreateAuthorizedAsync();

        var createVaultRes = await PostRequestAsync(ApiUrl.Vaults, jwtToken1, new CreateVaultRequest
        {
            Name = "Owner Vault",
            EncryptedVaultKey = new byte[] { 1, 2, 3 }
        });
        var vault = (await createVaultRes.Content.ReadFromJsonAsync<VaultResponse>())!.Vault;

        var createCredRes = await PostRequestAsync(ApiUrl.VaultCredentials, jwtToken1, new CreateCredentialRequest
        {
            VaultId = vault.Id,
            Type = CredentialType.Login,
            EncryptedBody = new byte[] { 4, 5, 6 }
        });
        var cred = (await createCredRes.Content.ReadFromJsonAsync<CredentialResponse>())!.Credential;

        // 2. User 2 tries to archive User 1's credential
        var archiveRes = await PostRequestAsync(ApiUrl.VaultCredentialArchive(cred.Id), jwtToken2, new { });
        Assert.Equal(HttpStatusCode.BadRequest, archiveRes.StatusCode);

        // 3. User 2 tries to get archived credentials from User 1's vault
        var getArchivedRes = await GetRequestAsync(ApiUrl.VaultArchivedCredentials(vault.Id), jwtToken2);
        Assert.Equal(HttpStatusCode.BadRequest, getArchivedRes.StatusCode);
    }
}

