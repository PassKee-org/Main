using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using PassKee.Api.Shared.Constants;
using PassKee.Api.Shared.Dto.RequestsAndResponses.Auth;
using PassKee.Api.Shared.Models.Vaults;
using PassKee.Api.Shared.Models.Vaults.Enums;
using PassKee.Business.Common.Constants.Http;
using PassKee.Business.Common.Utils;
using PassKee.Business.Testing.Extensions;
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
        // 1. Arrange & Auth
        var email = $"test_{Guid.NewGuid():N}@example.com";
        var password = "SecurePassword#2026";
        var regData = CryptoUtils.PrepareClientRegistration(password);

        var regRequest = new RegisterRequest
        {
            Email = email,
            AuthHash = regData.AuthHash,
            AuthSalt = regData.AuthSalt,
            UserPublicKey = regData.KeyEnvelope.PublicKey,
            EncryptedUserPrivateKey = regData.KeyEnvelope.EncryptedPrivateKey,
            EncryptedUserVaultKey = regData.KeyEnvelope.EncryptedVaultKey
        };
        var regResponse = await PostRequestAsAnonymousAsync(ApiUrl.AuthRegister, regRequest);
        Assert.Equal(HttpStatusCode.OK, regResponse.StatusCode);

        var jwtToken = regResponse.GetSetCookieValue(HttpCookieKeyEnum.JwtToken.GetKey());
        Assert.NotNull(jwtToken);
        
        // 1.5 Create Vault via API
        var createVaultReq = new CreateVaultRequest
        {
            Name = "API Created Vault",
            EncryptedVaultKey = new byte[] { 10, 20, 30 }
        };
        var createVaultRes = await PostRequestAsync("/api/vaults", jwtToken!, createVaultReq);
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
        var createDirRes = await PostRequestAsync("/api/vaults/directories", jwtToken!, createDirReq);
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
        var createCredRes = await PostRequestAsync("/api/vaults/credentials", jwtToken!, createCredReq);
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
        HttpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwtToken);
        var updateCredRes = await HttpClient.PutAsJsonAsync($"/api/vaults/credentials/{credId}", updateCredReq);
        var err3 = await updateCredRes.Content.ReadAsStringAsync();
        Assert.True(updateCredRes.StatusCode == HttpStatusCode.OK, err3);

        // 5. Delete Credential
        var deleteCredRes = await HttpClient.DeleteAsync($"/api/vaults/credentials/{credId}");
        Assert.Equal(HttpStatusCode.OK, deleteCredRes.StatusCode);

        // 6. Delete Directory
        var deleteDirRes = await HttpClient.DeleteAsync($"/api/vaults/directories/{dirId}");
        Assert.Equal(HttpStatusCode.OK, deleteDirRes.StatusCode);
    }
}
