namespace PassKee.Business.Common.Constants;

public static class FileStorageConstants
{
    /// <summary>
    /// Maximum size of a plain file selected by the user.
    /// </summary>
    public const int MaxFileSize = 20 * 1024 * 1024;

    /// <summary>
    /// Size added by AES-256-GCM client-side encryption: 12-byte nonce + 16-byte authentication tag.
    /// </summary>
    public const int EncryptionOverhead = 12 + 16;

    /// <summary>
    /// Maximum size of the blob accepted by the server (encrypted file).
    /// </summary>
    public const int MaxEncryptedFileSize = MaxFileSize + EncryptionOverhead;

    /// <summary>
    /// Request size limit for the upload endpoint: the encrypted blob plus multipart form overhead.
    /// </summary>
    public const int MaxUploadRequestSize = MaxEncryptedFileSize + 64 * 1024;

    /// <summary>
    /// Cloud objects hold encrypted data only, so a neutral extension is used instead of the original one.
    /// </summary>
    public const string CloudFileExtension = "bin";
}
