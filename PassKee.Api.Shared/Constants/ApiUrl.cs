using System;

namespace PassKee.Api.Shared.Constants;

public class ApiUrl
{
    public const string Ping = "api/ping";
    public const string AuthRegister = "api/auth/register";
    public const string AuthLogin = "api/auth/login";
    public const string AuthLoginParams = "api/auth/login-params";
    public const string AuthCheck = "api/auth/check";
    public const string AuthLogout = "api/auth/logout";

    public const string Vaults = "api/vaults";
    public static string VaultDetails(Guid vaultId) => $"api/vaults/{vaultId}";
    public const string VaultDirectories = "api/vaults/directories";
    public static string VaultDirectory(Guid directoryId) => $"api/vaults/directories/{directoryId}";
    public const string VaultCredentials = "api/vaults/credentials";
    public static string VaultCredential(Guid credentialId) => $"api/vaults/credentials/{credentialId}";
    public static string VaultCredentialArchive(Guid credentialId) => $"api/vaults/credentials/{credentialId}/archive";
    public static string VaultArchivedCredentials(Guid vaultId) => $"api/vaults/{vaultId}/credentials/archived";
    public const string VaultTags = "api/vaults/tags";
    public static string VaultTag(Guid tagId) => $"api/vaults/tags/{tagId}";

    public const string StorageUpload = "api/storage/upload";
    public static string StorageFile(Guid fileId) => $"api/storage/file/{fileId}";
}

