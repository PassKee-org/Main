using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace PassKee.Business.Services.Storage.Client;

public interface IFileStorageGarageClient
{
    Task UploadAsync(string filePath, Stream fileStream, CancellationToken cancellationToken = default);
    Task<Stream> GetAsStreamAsync(string filePath, CancellationToken cancellationToken = default);
    Task DeleteAsync(string filePath, CancellationToken cancellationToken = default);
}
