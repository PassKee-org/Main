using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using PassKee.Api.Shared.Constants;
using PassKee.Api.Shared.Models.Storage;
using PassKee.Api.Shared.Models.Storage.Requests;

namespace PassKee.Web.Services.Http;

public partial class ApiService
{
    public async Task<StoredFileDto?> UploadVaultFileAsync(Guid vaultId, byte[] encryptedFileBytes, string originalFileName)
    {
        using var content = new MultipartFormDataContent();
        
        content.Add(new StringContent(vaultId.ToString()), "EntityId");
        content.Add(new StringContent(((int)StorageEntityType.Vault).ToString()), "EntityType");
        
        var streamContent = new ByteArrayContent(encryptedFileBytes);
        content.Add(streamContent, "File", originalFileName);

        return await _httpClient.PostMultipartAsync<StoredFileDto>(ApiUrl.StorageUpload, content);
    }

    public async Task<byte[]?> DownloadFileAsync(Guid fileId)
    {
        try
        {
            return await _httpClient.GetByteArrayAsync(ApiUrl.StorageFile(fileId));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Download file error: {ex.Message}");
        }
        return null;
    }

    public async Task<bool> DeleteFileAsync(Guid fileId)
    {
        var response = await DeleteAsync<Api.Shared.Models.Vaults.ActionResponse>(ApiUrl.StorageFile(fileId));
        return response?.Success ?? false;
    }
}

