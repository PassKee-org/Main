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

        // 5.1 Verify that re-archiving or sharing archived cred1 is rejected, while editing is permitted
        var reArchiveRes = await PostRequestAsync(ApiUrl.VaultCredentialArchive(cred1Id), jwtToken, new { });
        Assert.Equal(HttpStatusCode.BadRequest, reArchiveRes.StatusCode);

        var updateArchivedRes = await PutRequestAsync(ApiUrl.VaultCredential(cred1Id), jwtToken, new UpdateCredentialRequest
        {
            CredentialId = cred1Id,
            DirectoryId = dirId,
            Type = CredentialType.Password,
            EncryptedBody = new byte[] { 111, 112 }
        });
        Assert.Equal(HttpStatusCode.OK, updateArchivedRes.StatusCode);
        var updatedArchivedResponse = await updateArchivedRes.Content.ReadFromJsonAsync<CredentialResponse>();
        Assert.NotNull(updatedArchivedResponse?.Credential);
        Assert.NotNull(updatedArchivedResponse.Credential.ArchivedAt);
        Assert.Equal(new byte[] { 111, 112 }, updatedArchivedResponse.Credential.EncryptedBody);

        var shareArchivedRes = await PostRequestAsync(ApiUrl.Shares, jwtToken, new PassKee.Api.Shared.Models.Shares.CreateCredentialShareRequest
        {
            CredentialId = cred1Id,
            EncryptedBody = new byte[] { 1, 2, 3 }
        });
        Assert.Equal(HttpStatusCode.BadRequest, shareArchivedRes.StatusCode);

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

        // 8. Delete directory and verify cred1 is NOT deleted, cred2 is archived, and both retain their DirectoryId
        var deleteDirRes = await DeleteRequestAsync(ApiUrl.VaultDirectory(dirId), jwtToken);
        Assert.Equal(HttpStatusCode.OK, deleteDirRes.StatusCode);
        var deleteDirResponse = await deleteDirRes.Content.ReadFromJsonAsync<DeleteDirectoryResponse>();
        Assert.NotNull(deleteDirResponse);
        Assert.Single(deleteDirResponse!.ArchivedCredentialIds);
        Assert.Contains(cred2Id, deleteDirResponse.ArchivedCredentialIds);

        var archivedAfterDirDeleteRes = await GetRequestAsync(ApiUrl.VaultArchivedCredentials(vaultId), jwtToken);
        Assert.Equal(HttpStatusCode.OK, archivedAfterDirDeleteRes.StatusCode);
        var archivedAfterDirDelete = await archivedAfterDirDeleteRes.Content.ReadFromJsonAsync<ArchivedCredentialsResponse>();
        Assert.NotNull(archivedAfterDirDelete);
        Assert.Equal(2, archivedAfterDirDelete!.Credentials.Count);
        Assert.Contains(archivedAfterDirDelete.Credentials, c => c.Id == cred1Id && c.DirectoryId == dirId && c.ArchivedAt != null);
        Assert.Contains(archivedAfterDirDelete.Credentials, c => c.Id == cred2Id && c.DirectoryId == dirId && c.ArchivedAt != null);

        // 9. Delete archived credentials
        var deleteCred1Res = await DeleteRequestAsync(ApiUrl.VaultCredential(cred1Id), jwtToken);
        Assert.Equal(HttpStatusCode.OK, deleteCred1Res.StatusCode);
        var deleteCred2Res = await DeleteRequestAsync(ApiUrl.VaultCredential(cred2Id), jwtToken);
        Assert.Equal(HttpStatusCode.OK, deleteCred2Res.StatusCode);

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

    [Fact]
    public async Task Can_Restore_Archived_Credential_To_Root_Directory()
    {
        // 1. Arrange & Auth
        var (jwtToken, user) = await UserSeeder.CreateAuthorizedAsync();

        // 2. Create Vault
        var createVaultRes = await PostRequestAsync(ApiUrl.Vaults, jwtToken, new CreateVaultRequest
        {
            Name = "Restore Vault",
            EncryptedVaultKey = new byte[] { 1, 2, 3 }
        });
        var vault = (await createVaultRes.Content.ReadFromJsonAsync<VaultResponse>())!.Vault;

        // 3. Create Directory
        var createDirRes = await PostRequestAsync(ApiUrl.VaultDirectories, jwtToken, new CreateDirectoryRequest
        {
            VaultId = vault.Id,
            ParentDirectoryId = null,
            EncryptedName = new byte[] { 4, 5, 6 }
        });
        var dir = (await createDirRes.Content.ReadFromJsonAsync<DirectoryResponse>())!.Directory;

        // 4. Create Credential inside Directory
        var createCredRes = await PostRequestAsync(ApiUrl.VaultCredentials, jwtToken, new CreateCredentialRequest
        {
            VaultId = vault.Id,
            DirectoryId = dir.Id,
            Type = CredentialType.Login,
            EncryptedBody = new byte[] { 7, 8, 9 }
        });
        var cred = (await createCredRes.Content.ReadFromJsonAsync<CredentialResponse>())!.Credential;
        Assert.Equal(dir.Id, cred.DirectoryId);

        // 5. Attempting to restore non-archived credential should fail (BadRequest)
        var preRestoreRes = await PostRequestAsync(ApiUrl.VaultCredentialRestore(cred.Id), jwtToken, new { });
        Assert.Equal(HttpStatusCode.BadRequest, preRestoreRes.StatusCode);

        // 6. Archive credential
        var archiveRes = await PostRequestAsync(ApiUrl.VaultCredentialArchive(cred.Id), jwtToken, new { });
        Assert.Equal(HttpStatusCode.OK, archiveRes.StatusCode);
        var archivedCred = (await archiveRes.Content.ReadFromJsonAsync<CredentialResponse>())!.Credential;
        Assert.NotNull(archivedCred.ArchivedAt);
        Assert.Equal(dir.Id, archivedCred.DirectoryId);

        // Verify it is in archived list, and NOT in vault details
        var vaultDetailsBeforeRestore = (await (await GetRequestAsync(ApiUrl.VaultDetails(vault.Id), jwtToken)).Content.ReadFromJsonAsync<VaultDetailsResponse>())!;
        Assert.DoesNotContain(vaultDetailsBeforeRestore.Credentials, c => c.Id == cred.Id);

        var archivedBeforeRestore = (await (await GetRequestAsync(ApiUrl.VaultArchivedCredentials(vault.Id), jwtToken)).Content.ReadFromJsonAsync<ArchivedCredentialsResponse>())!;
        Assert.Contains(archivedBeforeRestore.Credentials, c => c.Id == cred.Id);

        // 7. Restore credential
        var restoreRes = await PostRequestAsync(ApiUrl.VaultCredentialRestore(cred.Id), jwtToken, new { });
        Assert.Equal(HttpStatusCode.OK, restoreRes.StatusCode);
        var restoredCred = (await restoreRes.Content.ReadFromJsonAsync<CredentialResponse>())!.Credential;

        // Requirement: ArchivedAt == null, and sent to root directory (DirectoryId == null)
        Assert.Null(restoredCred.ArchivedAt);
        Assert.Null(restoredCred.DirectoryId);

        // 8. Verify VaultDetails now has the restored credential in root (DirectoryId == null)
        var vaultDetailsAfterRestore = (await (await GetRequestAsync(ApiUrl.VaultDetails(vault.Id), jwtToken)).Content.ReadFromJsonAsync<VaultDetailsResponse>())!;
        var foundInVault = Assert.Single(vaultDetailsAfterRestore.Credentials, c => c.Id == cred.Id);
        Assert.Null(foundInVault.DirectoryId);
        Assert.Null(foundInVault.ArchivedAt);

        // 9. Verify ArchivedCredentials no longer has the restored credential
        var archivedAfterRestore = (await (await GetRequestAsync(ApiUrl.VaultArchivedCredentials(vault.Id), jwtToken)).Content.ReadFromJsonAsync<ArchivedCredentialsResponse>())!;
        Assert.DoesNotContain(archivedAfterRestore.Credentials, c => c.Id == cred.Id);

        // 10. Attempting to restore already restored credential should fail
        var repeatRestoreRes = await PostRequestAsync(ApiUrl.VaultCredentialRestore(cred.Id), jwtToken, new { });
        Assert.Equal(HttpStatusCode.BadRequest, repeatRestoreRes.StatusCode);
    }

    [Fact]
    public async Task Non_Owner_Cannot_Restore_Credential()
    {
        // 1. User 1 creates vault, credential, and archives it
        var (jwtToken1, user1) = await UserSeeder.CreateAuthorizedAsync();
        var (jwtToken2, user2) = await UserSeeder.CreateAuthorizedAsync();

        var createVaultRes = await PostRequestAsync(ApiUrl.Vaults, jwtToken1, new CreateVaultRequest
        {
            Name = "Owner Vault 2",
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

        var archiveRes = await PostRequestAsync(ApiUrl.VaultCredentialArchive(cred.Id), jwtToken1, new { });
        Assert.Equal(HttpStatusCode.OK, archiveRes.StatusCode);

        // 2. User 2 tries to restore User 1's credential
        var restoreRes = await PostRequestAsync(ApiUrl.VaultCredentialRestore(cred.Id), jwtToken2, new { });
        Assert.Equal(HttpStatusCode.BadRequest, restoreRes.StatusCode);
    }
}

