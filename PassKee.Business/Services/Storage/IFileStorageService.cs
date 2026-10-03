using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Domain.Abstractions;
using Microsoft.AspNetCore.Http;
using PassKee.Orm.Entities.Storage;

namespace PassKee.Business.Services.Storage;

/// <summary>
/// Common file storage: stores encrypted blobs in the cloud and binds them to an owning entity.
/// Access checks are the responsibility of the caller.
/// </summary>
public interface IFileStorageService : IDomainService
{
    /// <summary>
    /// Validates and stores the file for the given owner entity. The stored file type is chosen by the owner type.
    /// </summary>
    Task<FileStorageEntity> PutFileAsync<TEntity>(
        TEntity entity,
        IFormFile formFile,
        CancellationToken cancellationToken = default) where TEntity : IEntity;

    Task<Stream> GetFileStreamAsync(FileStorageEntity file, CancellationToken cancellationToken = default);

    Task DeleteFileAsync(FileStorageEntity file, CancellationToken cancellationToken = default);
}
