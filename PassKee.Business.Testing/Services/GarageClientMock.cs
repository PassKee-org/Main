using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using PassKee.Business.Services.Storage.Client;

namespace PassKee.Business.Testing.Services;

/// <summary>
/// In-memory replacement of the Garage client: keeps uploaded blobs so tests can assert on cloud state.
/// </summary>
public class GarageClientMock : IFileStorageGarageClient
{
    public ConcurrentDictionary<string, byte[]> Files { get; } = new();

    public async Task UploadAsync(string filePath, Stream fileStream, CancellationToken cancellationToken = default)
    {
        using var memoryStream = new MemoryStream();
        await fileStream.CopyToAsync(memoryStream, cancellationToken);
        Files[filePath] = memoryStream.ToArray();
    }

    public Task<Stream> GetAsStreamAsync(string filePath, CancellationToken cancellationToken = default)
    {
        if (!Files.TryGetValue(filePath, out var bytes))
        {
            throw new FileNotFoundException($"S3 File not found: {filePath}");
        }

        return Task.FromResult<Stream>(new MemoryStream(bytes));
    }

    public Task DeleteAsync(string filePath, CancellationToken cancellationToken = default)
    {
        Files.TryRemove(filePath, out _);
        return Task.CompletedTask;
    }
}
