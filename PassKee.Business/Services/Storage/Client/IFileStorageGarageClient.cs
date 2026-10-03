using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Domain.Abstractions;

namespace PassKee.Business.Services.Storage.Client;

public interface IFileStorageGarageClient : IDomainService
{
    Task UploadAsync(string filePath, Stream fileStream, CancellationToken cancellationToken = default);
    Task<Stream> GetAsStreamAsync(string filePath, CancellationToken cancellationToken = default);
    Task DeleteAsync(string filePath, CancellationToken cancellationToken = default);
}
