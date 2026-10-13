using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using NHibernate.Linq;
using PassKee.Api.Shared.Constants;
using PassKee.Api.Shared.Models.Vaults;
using PassKee.Orm.Entities.Vaults;
using PassKee.Tests.Integration.Api.Core;
using Xunit;

namespace PassKee.Tests.Integration.Api.Controllers.Vaults;

public class DirectoriesMoveTests : BaseTest
{
    public DirectoriesMoveTests(ApiCustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Can_Move_Directory_To_Another_Directory_And_Then_To_Root()
    {
        // 1. Arrange & Auth
        var (jwtToken, _) = await UserSeeder.CreateAuthorizedAsync();

        // 2. Create Vault
        var createVaultRes = await PostRequestAsync(ApiUrl.Vaults, jwtToken, new CreateVaultRequest
        {
            Name = "Move Test Vault",
            EncryptedVaultKey = [1, 2, 3]
        });
        Assert.Equal(HttpStatusCode.OK, createVaultRes.StatusCode);
        var vault = (await createVaultRes.Content.ReadFromJsonAsync<VaultResponse>())!.Vault;

        // 3. Create Dir A (Root) and Dir B (Root)
        var dirARes = await PostRequestAsync(ApiUrl.VaultDirectories, jwtToken, new CreateDirectoryRequest
        {
            VaultId = vault.Id,
            ParentDirectoryId = null,
            EncryptedName = [10, 20]
        });
        Assert.Equal(HttpStatusCode.OK, dirARes.StatusCode);
        var dirA = (await dirARes.Content.ReadFromJsonAsync<DirectoryResponse>())!.Directory;

        var dirBRes = await PostRequestAsync(ApiUrl.VaultDirectories, jwtToken, new CreateDirectoryRequest
        {
            VaultId = vault.Id,
            ParentDirectoryId = null,
            EncryptedName = [30, 40]
        });
        Assert.Equal(HttpStatusCode.OK, dirBRes.StatusCode);
        var dirB = (await dirBRes.Content.ReadFromJsonAsync<DirectoryResponse>())!.Directory;

        // 4. Create Dir C inside Dir A
        var dirCRes = await PostRequestAsync(ApiUrl.VaultDirectories, jwtToken, new CreateDirectoryRequest
        {
            VaultId = vault.Id,
            ParentDirectoryId = dirA.Id,
            EncryptedName = [50, 60]
        });
        Assert.Equal(HttpStatusCode.OK, dirCRes.StatusCode);
        var dirC = (await dirCRes.Content.ReadFromJsonAsync<DirectoryResponse>())!.Directory;
        Assert.Equal(dirA.Id, dirC.ParentDirectoryId);

        // 5. Move Dir C from Dir A to Dir B via POST api/vaults/directories/move
        var moveReq = new MoveDirectoryRequest
        {
            DirectoryId = dirC.Id,
            ParentDirectoryId = dirB.Id
        };
        var moveRes = await PostRequestAsync(ApiUrl.VaultDirectoryMove, jwtToken, moveReq);
        Assert.Equal(HttpStatusCode.OK, moveRes.StatusCode);
        var movedC = (await moveRes.Content.ReadFromJsonAsync<DirectoryResponse>())!.Directory;
        Assert.Equal(dirB.Id, movedC.ParentDirectoryId);

        // Verify in DB directly
        var dirCInDb = await DbSessionProvider.CurrentSession.Query<DirectoryEntity>()
            .FirstOrDefaultAsync(d => d.Id == dirC.Id);
        Assert.NotNull(dirCInDb);
        Assert.Equal(dirB.Id, dirCInDb.ParentDirectoryId);

        // 6. Move Dir C from Dir B to Root (null) via POST api/vaults/directories/move
        var moveToRootReq = new MoveDirectoryRequest
        {
            DirectoryId = dirC.Id,
            ParentDirectoryId = null
        };
        var moveToRootRes = await PostRequestAsync(ApiUrl.VaultDirectoryMove, jwtToken, moveToRootReq);
        Assert.Equal(HttpStatusCode.OK, moveToRootRes.StatusCode);
        var rootC = (await moveToRootRes.Content.ReadFromJsonAsync<DirectoryResponse>())!.Directory;
        Assert.Null(rootC.ParentDirectoryId);

        // Verify in DB directly
        DbSessionProvider.CurrentSession.Clear();
        var rootCInDb = await DbSessionProvider.CurrentSession.Query<DirectoryEntity>()
            .FirstOrDefaultAsync(d => d.Id == dirC.Id);
        Assert.NotNull(rootCInDb);
        Assert.Null(rootCInDb.ParentDirectoryId);
    }

    [Fact]
    public async Task Cannot_Move_Directory_Into_Itself()
    {
        var (jwtToken, _) = await UserSeeder.CreateAuthorizedAsync();

        var createVaultRes = await PostRequestAsync(ApiUrl.Vaults, jwtToken, new CreateVaultRequest
        {
            Name = "Self Cycle Vault",
            EncryptedVaultKey = [1, 2, 3]
        });
        var vault = (await createVaultRes.Content.ReadFromJsonAsync<VaultResponse>())!.Vault;

        var dirARes = await PostRequestAsync(ApiUrl.VaultDirectories, jwtToken, new CreateDirectoryRequest
        {
            VaultId = vault.Id,
            ParentDirectoryId = null,
            EncryptedName = [10]
        });
        var dirA = (await dirARes.Content.ReadFromJsonAsync<DirectoryResponse>())!.Directory;

        var moveReq = new MoveDirectoryRequest
        {
            DirectoryId = dirA.Id,
            ParentDirectoryId = dirA.Id
        };
        var moveRes = await PostRequestAsync(ApiUrl.VaultDirectoryMove, jwtToken, moveReq);
        Assert.Equal(HttpStatusCode.BadRequest, moveRes.StatusCode);
    }

    [Fact]
    public async Task Cannot_Move_Directory_Into_Descendant()
    {
        var (jwtToken, _) = await UserSeeder.CreateAuthorizedAsync();

        var createVaultRes = await PostRequestAsync(ApiUrl.Vaults, jwtToken, new CreateVaultRequest
        {
            Name = "Descendant Cycle Vault",
            EncryptedVaultKey = [1, 2, 3]
        });
        var vault = (await createVaultRes.Content.ReadFromJsonAsync<VaultResponse>())!.Vault;

        // Dir A -> Dir B -> Dir C
        var dirARes = await PostRequestAsync(ApiUrl.VaultDirectories, jwtToken, new CreateDirectoryRequest
        {
            VaultId = vault.Id,
            ParentDirectoryId = null,
            EncryptedName = [10]
        });
        var dirA = (await dirARes.Content.ReadFromJsonAsync<DirectoryResponse>())!.Directory;

        var dirBRes = await PostRequestAsync(ApiUrl.VaultDirectories, jwtToken, new CreateDirectoryRequest
        {
            VaultId = vault.Id,
            ParentDirectoryId = dirA.Id,
            EncryptedName = [20]
        });
        var dirB = (await dirBRes.Content.ReadFromJsonAsync<DirectoryResponse>())!.Directory;

        var dirCRes = await PostRequestAsync(ApiUrl.VaultDirectories, jwtToken, new CreateDirectoryRequest
        {
            VaultId = vault.Id,
            ParentDirectoryId = dirB.Id,
            EncryptedName = [30]
        });
        var dirC = (await dirCRes.Content.ReadFromJsonAsync<DirectoryResponse>())!.Directory;

        // Try to move Dir A into child Dir B
        var moveAtoB = new MoveDirectoryRequest
        {
            DirectoryId = dirA.Id,
            ParentDirectoryId = dirB.Id
        };
        var resAtoB = await PostRequestAsync(ApiUrl.VaultDirectoryMove, jwtToken, moveAtoB);
        Assert.Equal(HttpStatusCode.BadRequest, resAtoB.StatusCode);

        // Try to move Dir A into grandchild Dir C
        var moveAtoC = new MoveDirectoryRequest
        {
            DirectoryId = dirA.Id,
            ParentDirectoryId = dirC.Id
        };
        var resAtoC = await PostRequestAsync(ApiUrl.VaultDirectoryMove, jwtToken, moveAtoC);
        Assert.Equal(HttpStatusCode.BadRequest, resAtoC.StatusCode);
    }

    [Fact]
    public async Task Cannot_Move_Directory_To_Another_Vault()
    {
        var (jwtToken, _) = await UserSeeder.CreateAuthorizedAsync();

        var vault1Res = await PostRequestAsync(ApiUrl.Vaults, jwtToken, new CreateVaultRequest
        {
            Name = "Vault 1",
            EncryptedVaultKey = [1]
        });
        var vault1 = (await vault1Res.Content.ReadFromJsonAsync<VaultResponse>())!.Vault;

        var vault2Res = await PostRequestAsync(ApiUrl.Vaults, jwtToken, new CreateVaultRequest
        {
            Name = "Vault 2",
            EncryptedVaultKey = [2]
        });
        var vault2 = (await vault2Res.Content.ReadFromJsonAsync<VaultResponse>())!.Vault;

        var dir1Res = await PostRequestAsync(ApiUrl.VaultDirectories, jwtToken, new CreateDirectoryRequest
        {
            VaultId = vault1.Id,
            ParentDirectoryId = null,
            EncryptedName = [10]
        });
        var dir1 = (await dir1Res.Content.ReadFromJsonAsync<DirectoryResponse>())!.Directory;

        var dir2Res = await PostRequestAsync(ApiUrl.VaultDirectories, jwtToken, new CreateDirectoryRequest
        {
            VaultId = vault2.Id,
            ParentDirectoryId = null,
            EncryptedName = [20]
        });
        var dir2 = (await dir2Res.Content.ReadFromJsonAsync<DirectoryResponse>())!.Directory;

        var moveCrossVault = new MoveDirectoryRequest
        {
            DirectoryId = dir1.Id,
            ParentDirectoryId = dir2.Id
        };
        var res = await PostRequestAsync(ApiUrl.VaultDirectoryMove, jwtToken, moveCrossVault);
        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
    }
}
