using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NHibernate.Linq;
using PassKee.Api.Shared.Constants;
using PassKee.Api.Shared.Models.Storage;
using PassKee.Api.Shared.Models.Storage.Requests;
using PassKee.Api.Shared.Models.Vaults;
using PassKee.Business.Common.Constants;
using PassKee.Business.Common.Exceptions.Api;
using PassKee.Business.Services.Storage.Client;
using PassKee.Orm.Entities.Storage;
using PassKee.Tests.Integration.Api.Core;
using Xunit;

namespace PassKee.Tests.Integration.Api.Controllers.Storage;

public class StorageControllerTests : BaseTest
{
    private static readonly string UploadUrl = ApiUrl.StorageUpload;

    private readonly IFileStorageGarageClient _garage;

    public StorageControllerTests(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        _garage = ServiceProvider.GetRequiredService<IFileStorageGarageClient>();
    }

    [Fact]
    public async Task Can_Upload_Get_And_Delete_File()
    {
        var (jwtToken, _) = await UserSeeder.CreateAuthorizedAsync();
        var vaultId = await CreateVaultAsync(jwtToken);
        var content = new byte[] { 5, 6, 7, 8 };

        var uploadRes = await UploadAsync(jwtToken, vaultId, content);
        Assert.Equal(HttpStatusCode.OK, uploadRes.StatusCode);
        var storedFile = (await uploadRes.Content.ReadFromJsonAsync<StoredFileDto>())!;
        Assert.Equal(vaultId, storedFile.VaultId);
        Assert.Equal(content.Length, storedFile.Size);

        var dbFile = await GetVaultFileFromDbAsync(storedFile.Id);
        Assert.NotNull(dbFile);
        Assert.Equal(vaultId, dbFile.VaultId);
        Assert.Equal(content.Length, dbFile.Size);
        Assert.StartsWith($"vault/{vaultId}/", dbFile.CloudFilePath);
        Assert.EndsWith(".bin", dbFile.CloudFilePath);

        using (var s3Stream = await _garage.GetAsStreamAsync(dbFile.CloudFilePath))
        using (var ms = new MemoryStream())
        {
            await s3Stream.CopyToAsync(ms);
            Assert.Equal(content, ms.ToArray());
        }

        var getRes = await GetRequestAsync(ApiUrl.StorageFile(storedFile.Id), jwtToken);
        Assert.Equal(HttpStatusCode.OK, getRes.StatusCode);
        Assert.Equal(content, await getRes.Content.ReadAsByteArrayAsync());

        var deleteRes = await DeleteRequestAsync(ApiUrl.StorageFile(storedFile.Id), jwtToken);
        Assert.Equal(HttpStatusCode.OK, deleteRes.StatusCode);
        Assert.Null(await GetVaultFileFromDbAsync(storedFile.Id));
        await Assert.ThrowsAsync<RecordNotFoundException>(() => _garage.GetAsStreamAsync(dbFile.CloudFilePath));
    }

    [Fact]
    public async Task Upload_As_Anonymous_Returns_Unauthorized()
    {
        var (jwtToken, _) = await UserSeeder.CreateAuthorizedAsync();
        var vaultId = await CreateVaultAsync(jwtToken);

        // A fresh client: the shared one keeps the Authorization header set by earlier requests.
        using var anonymousClient = _factory.CreateClient();
        var response = await anonymousClient.SendAsync(CreateUploadRequest(null, vaultId, new byte[] { 1 }));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Empty(await DbSessionProvider.CurrentSession.Query<FileStorageEntity>().ToListAsync());
    }

    [Fact]
    public async Task Other_User_Cannot_Upload_Get_Or_Delete_Files_Of_Foreign_Vault()
    {
        var (ownerToken, _) = await UserSeeder.CreateAuthorizedAsync();
        var (strangerToken, _) = await UserSeeder.CreateAuthorizedAsync();
        var vaultId = await CreateVaultAsync(ownerToken);
        var storedFile = await UploadAndReadAsync(ownerToken, vaultId, new byte[] { 1, 2, 3 });

        var uploadRes = await UploadAsync(strangerToken, vaultId, new byte[] { 9 });
        await AssertNoAccessAsync(uploadRes);

        var getRes = await GetRequestAsync(ApiUrl.StorageFile(storedFile.Id), strangerToken);
        await AssertNoAccessAsync(getRes);

        var deleteRes = await DeleteRequestAsync(ApiUrl.StorageFile(storedFile.Id), strangerToken);
        await AssertNoAccessAsync(deleteRes);

        var ownerFile = await GetVaultFileFromDbAsync(storedFile.Id);
        Assert.NotNull(ownerFile);
        using (var s3Stream = await _garage.GetAsStreamAsync(ownerFile.CloudFilePath))
        {
            Assert.NotNull(s3Stream);
        }

        // Clean up
        await DeleteRequestAsync(ApiUrl.StorageFile(storedFile.Id), ownerToken);
    }

    [Fact]
    public async Task Upload_To_Missing_Vault_Returns_Not_Found()
    {
        var (jwtToken, _) = await UserSeeder.CreateAuthorizedAsync();

        var response = await UploadAsync(jwtToken, Guid.NewGuid(), new byte[] { 1 });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("RecordNotFoundException", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Get_And_Delete_Missing_File_Return_Not_Found()
    {
        var (jwtToken, _) = await UserSeeder.CreateAuthorizedAsync();
        var fileId = Guid.NewGuid();

        var getRes = await GetRequestAsync(ApiUrl.StorageFile(fileId), jwtToken);
        Assert.Equal(HttpStatusCode.BadRequest, getRes.StatusCode);
        Assert.Contains("RecordNotFoundException", await getRes.Content.ReadAsStringAsync());

        var deleteRes = await DeleteRequestAsync(ApiUrl.StorageFile(fileId), jwtToken);
        Assert.Equal(HttpStatusCode.BadRequest, deleteRes.StatusCode);
        Assert.Contains("RecordNotFoundException", await deleteRes.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Upload_Larger_Than_Limit_Is_Rejected()
    {
        var (jwtToken, _) = await UserSeeder.CreateAuthorizedAsync();
        var vaultId = await CreateVaultAsync(jwtToken);

        var response = await UploadAsync(jwtToken, vaultId, new byte[FileStorageConstants.MaxEncryptedFileSize + 1]);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("IncorrectFileException", await response.Content.ReadAsStringAsync());
        Assert.Empty(await DbSessionProvider.CurrentSession.Query<FileStorageEntity>().ToListAsync());
    }

    [Fact]
    public async Task Upload_Of_Maximum_Encrypted_Size_Is_Accepted()
    {
        var (jwtToken, _) = await UserSeeder.CreateAuthorizedAsync();
        var vaultId = await CreateVaultAsync(jwtToken);

        var response = await UploadAsync(jwtToken, vaultId, new byte[FileStorageConstants.MaxEncryptedFileSize]);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var storedFile = (await response.Content.ReadFromJsonAsync<StoredFileDto>())!;

        // Clean up large file from Garage
        await DeleteRequestAsync(ApiUrl.StorageFile(storedFile.Id), jwtToken);
    }

    [Fact]
    public async Task Upload_Empty_File_Is_Rejected()
    {
        var (jwtToken, _) = await UserSeeder.CreateAuthorizedAsync();
        var vaultId = await CreateVaultAsync(jwtToken);

        var response = await UploadAsync(jwtToken, vaultId, []);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Empty(await DbSessionProvider.CurrentSession.Query<FileStorageEntity>().ToListAsync());
    }

    [Fact]
    public async Task Upload_With_Unsupported_Entity_Type_Is_Rejected()
    {
        var (jwtToken, _) = await UserSeeder.CreateAuthorizedAsync();
        var vaultId = await CreateVaultAsync(jwtToken);

        var request = CreateUploadRequest(jwtToken, vaultId, new byte[] { 1 }, entityType: 99);
        var response = await HttpClient.SendAsync(request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Empty(await DbSessionProvider.CurrentSession.Query<FileStorageEntity>().ToListAsync());
    }

    private async Task<Guid> CreateVaultAsync(string jwtToken)
    {
        var response = await PostRequestAsync(ApiUrl.Vaults, jwtToken, new CreateVaultRequest
        {
            Name = "Storage Vault",
            EncryptedVaultKey = new byte[] { 1, 2, 3 }
        });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<VaultResponse>())!.Vault.Id;
    }

    private async Task<StoredFileDto> UploadAndReadAsync(string jwtToken, Guid vaultId, byte[] content)
    {
        var response = await UploadAsync(jwtToken, vaultId, content);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<StoredFileDto>())!;
    }

    private async Task<HttpResponseMessage> UploadAsync(string jwtToken, Guid vaultId, byte[] content)
    {
        await FlushDbChanges();
        return await HttpClient.SendAsync(CreateUploadRequest(jwtToken, vaultId, content));
    }

    private static HttpRequestMessage CreateUploadRequest(string? jwtToken, Guid vaultId, byte[] content, int entityType = (int)StorageEntityType.Vault)
    {
        var form = new MultipartFormDataContent
        {
            { new StringContent(vaultId.ToString()), nameof(UploadRequest.EntityId) },
            { new StringContent(entityType.ToString()), nameof(UploadRequest.EntityType) }
        };
        var fileContent = new ByteArrayContent(content);
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/octet-stream");
        form.Add(fileContent, nameof(UploadRequest.File), "encrypted.bin");

        var request = new HttpRequestMessage(HttpMethod.Post, UploadUrl) { Content = form };
        if (jwtToken != null)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
        }

        return request;
    }

    private async Task<VaultFileStorageEntity?> GetVaultFileFromDbAsync(Guid fileId)
    {
        DbSessionProvider.CurrentSession.Clear();
        return await DbSessionProvider.CurrentSession.Query<VaultFileStorageEntity>()
            .FirstOrDefaultAsync(x => x.Id == fileId);
    }

    private static async Task AssertNoAccessAsync(HttpResponseMessage response)
    {
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("HasNoAccessException", await response.Content.ReadAsStringAsync());
    }
}
