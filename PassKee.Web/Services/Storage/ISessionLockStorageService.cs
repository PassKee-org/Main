using System.Threading.Tasks;
using PassKee.Business.Common.Utils;

namespace PassKee.Web.Services.Storage;

public class SessionLockInfo
{
    public string Email { get; set; } = string.Empty;
    public string AuthSaltBase64 { get; set; } = string.Empty;
    public string EncryptedSecretKeyBase64 { get; set; } = string.Empty;
    public string? EncryptedUserPrivateKeyBase64 { get; set; }
    public int Iterations { get; set; } = CryptoUtils.DefaultKdfIterations;
    public int MemorySize { get; set; } = CryptoUtils.DefaultKdfMemorySize;
    public int Parallelism { get; set; } = CryptoUtils.DefaultKdfParallelism;
    public bool IsLocked { get; set; }
}

public interface ISessionLockStorageService
{
    Task<SessionLockInfo?> GetSessionLockInfoAsync();
    Task SetSessionLockInfoAsync(SessionLockInfo info);
    Task ClearSessionLockInfoAsync();
    Task LockSessionAsync();
    Task ClearAllSessionDataAsync();
}
