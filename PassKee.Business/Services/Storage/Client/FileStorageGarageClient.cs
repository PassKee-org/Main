using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace PassKee.Business.Services.Storage.Client;

public class FileStorageGarageClient : IFileStorageGarageClient
{
    private readonly ILogger<FileStorageGarageClient> _logger;
    private readonly string _bucketName;
    private readonly AmazonS3Client _s3Client;

    public FileStorageGarageClient(
        IConfiguration configuration,
        ILogger<FileStorageGarageClient> logger)
    {
        _logger = logger;

        var accessKey = configuration.GetValue<string>("Garage:AccessKey") 
            ?? throw new ArgumentNullException("Garage:AccessKey is missing in config");
        var secretKey = configuration.GetValue<string>("Garage:SecretKey") 
            ?? throw new ArgumentNullException("Garage:SecretKey is missing in config");
        _bucketName = configuration.GetValue<string>("Garage:BucketName") 
            ?? throw new ArgumentNullException("Garage:BucketName is missing in config");
        var url = configuration.GetValue<string>("Garage:Url") 
            ?? throw new ArgumentNullException("Garage:Url is missing in config");

        var config = new AmazonS3Config
        {
            ServiceURL = url,
            ForcePathStyle = true,
            DisableLogging = true,
            AuthenticationRegion = "garage"
        };
        _s3Client = new AmazonS3Client(accessKey, secretKey, config);
    }

    public async Task UploadAsync(string filePath, Stream fileStream, CancellationToken cancellationToken = default)
    {
        var s3Request = new PutObjectRequest
        {
            BucketName = _bucketName,
            Key = filePath,
            InputStream = fileStream,
            AutoCloseStream = false,
            DisableDefaultChecksumValidation = true
        };

        _logger.LogDebug($"S3 file uploading started: {filePath}");
        var response = await _s3Client.PutObjectAsync(s3Request, cancellationToken);
        if (response.HttpStatusCode != System.Net.HttpStatusCode.OK)
        {
            throw new Exception($"File uploading error via S3 client: {response.HttpStatusCode}");
        }
        _logger.LogDebug($"S3 file uploading finished: {filePath}");
    }

    public async Task<Stream> GetAsStreamAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var response = await _s3Client.GetObjectAsync(_bucketName, filePath, cancellationToken);
        if (response == null)
        {
            throw new Exception($"S3 File not found: {filePath}");
        }

        var fileStream = new MemoryStream();
        await response.ResponseStream.CopyToAsync(fileStream, cancellationToken);
        fileStream.Position = 0;
        return fileStream;
    }

    public async Task DeleteAsync(string filePath, CancellationToken cancellationToken = default)
    {
        await _s3Client.DeleteObjectAsync(_bucketName, filePath, cancellationToken);
    }
}
