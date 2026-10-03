using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Domain.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using PassKee.Business.Common.Constants;
using PassKee.Business.Common.Exceptions.Api;
using PassKee.Business.Services.Storage.Client;
using PassKee.Orm.Entities.Storage;
using PassKee.Orm.Entities.Vaults;
using Persistence.Transactions.Behaviors;

namespace PassKee.Business.Services.Storage;

public class FileStorageService : IFileStorageService
{
    private readonly IDbSessionProvider _dbSessionProvider;
    private readonly IFileStorageGarageClient _storageClient;
    private readonly ILogger<FileStorageService> _logger;

    public FileStorageService(
        IDbSessionProvider dbSessionProvider,
        IFileStorageGarageClient storageClient,
        ILogger<FileStorageService> logger)
    {
        _dbSessionProvider = dbSessionProvider;
        _storageClient = storageClient;
        _logger = logger;
    }

    public async Task<FileStorageEntity> PutFileAsync<TEntity>(
        TEntity entity,
        IFormFile formFile,
        CancellationToken cancellationToken = default) where TEntity : IEntity
    {
        ValidateFile(formFile);

        FileStorageEntity storedFile = entity switch
        {
            VaultEntity vault => new VaultFileStorageEntity { VaultId = vault.Id },
            _ => throw new NotSupportedException($"Files are not supported for {typeof(TEntity).Name}")
        };
        storedFile.Size = formFile.Length;
        storedFile.CloudFilePath = BuildCloudFilePath(entity);

        using var stream = new MemoryStream();
        await formFile.CopyToAsync(stream, cancellationToken);
        stream.Position = 0;

        // The record is saved first so that a failed upload rolls back the ambient transaction.
        await _dbSessionProvider.CurrentSession.SaveAsync(storedFile, cancellationToken);

        _logger.LogDebug("S3 file uploading started: {Path}", storedFile.CloudFilePath);
        await _storageClient.UploadAsync(storedFile.CloudFilePath, stream, cancellationToken);
        _logger.LogDebug("S3 file uploading finished: {Path}", storedFile.CloudFilePath);

        return storedFile;
    }

    public Task<Stream> GetFileStreamAsync(FileStorageEntity file, CancellationToken cancellationToken = default)
    {
        return _storageClient.GetAsStreamAsync(file.CloudFilePath, cancellationToken);
    }

    public async Task DeleteFileAsync(FileStorageEntity file, CancellationToken cancellationToken = default)
    {
        await _storageClient.DeleteAsync(file.CloudFilePath, cancellationToken);
        await _dbSessionProvider.CurrentSession.DeleteAsync(file, cancellationToken);
    }

    private static void ValidateFile(IFormFile formFile)
    {
        if (formFile.Length == 0)
        {
            throw new IncorrectFileException("File is empty");
        }

        if (formFile.Length > FileStorageConstants.MaxEncryptedFileSize)
        {
            throw new IncorrectFileException($"File can not be larger than {FileStorageConstants.MaxFileSize / 1024 / 1024}Mb");
        }
    }

    private static string BuildCloudFilePath<TEntity>(TEntity entity) where TEntity : IEntity
    {
        var parentDir = entity switch
        {
            VaultEntity => "vault",
            _ => "common"
        };
        return $"{parentDir}/{entity.Id}/{Guid.CreateVersion7()}.{FileStorageConstants.CloudFileExtension}";
    }
}
