using System;
using System.Threading.Tasks;
using PassKee.Api.Shared.Dto.RequestsAndResponses.Auth;
using PassKee.Business.Common.Utils;
using PassKee.Web.Services.Http;
using PassKee.Web.Services.Storage;

namespace PassKee.Web.Services.Auth;

public class AuthClientService : IAuthClientService
{
    private readonly ApiService _apiService;
    private readonly ISessionLockStorageService _sessionLockStorage;

    public AuthClientService(ApiService apiService, ISessionLockStorageService sessionLockStorage)
    {
        _apiService = apiService;
        _sessionLockStorage = sessionLockStorage;
    }

    public async Task<RegisterResult> RegisterAsync(string email, string password)
    {
        var regData = CryptoUtils.PrepareClientRegistration(password);
        var secretKeyBase64 = Convert.ToBase64String(regData.SecretKey);

        var request = new RegisterRequest
        {
            Email = email,
            AuthHash = regData.AuthHash,
            AuthSalt = regData.AuthSalt,
            KdfParams = new KdfParamsRequest
            {
                Iterations = regData.KdfParams.Iterations,
                MemorySize = regData.KdfParams.MemorySize,
                Parallelism = regData.KdfParams.Parallelism
            },
            UserPublicKey = regData.KeyEnvelope.PublicKey,
            EncryptedUserPrivateKey = regData.KeyEnvelope.EncryptedPrivateKey,
            EncryptedUserVaultKey = regData.KeyEnvelope.EncryptedVaultKey
        };

        var response = await _apiService.RegisterAsync(request);
        if (response == null)
        {
            throw new InvalidOperationException("Registration failed. Please try again.");
        }

        // Decrypt ECC private key for in-memory session use
        var userPrivateKey = CryptoUtils.AesGcmDecrypt(regData.MasterKey, regData.KeyEnvelope.EncryptedPrivateKey);

        // Store encrypted secret key in session storage to enable session unlock with Master Password only
        var encryptedSecretKey = CryptoUtils.EncryptSecretKeyForSession(regData.SecretKey, password, regData.AuthSalt);
        var sessionLock = new SessionLockInfo
        {
            Email = email,
            AuthSaltBase64 = Convert.ToBase64String(regData.AuthSalt),
            EncryptedSecretKeyBase64 = Convert.ToBase64String(encryptedSecretKey),
            EncryptedUserPrivateKeyBase64 = Convert.ToBase64String(regData.KeyEnvelope.EncryptedPrivateKey),
            Iterations = regData.KdfParams.Iterations,
            MemorySize = regData.KdfParams.MemorySize,
            Parallelism = regData.KdfParams.Parallelism,
            IsLocked = false
        };
        await _sessionLockStorage.SetSessionLockInfoAsync(sessionLock);

        return new RegisterResult(response, secretKeyBase64, userPrivateKey, regData.KeyEnvelope.PublicKey);
    }

    public async Task<LoginResult> LoginAsync(string email, string password, string secretKeyBase64)
    {
        var loginParams = await _apiService.GetLoginParamsAsync(email);
        if (loginParams == null || string.IsNullOrEmpty(loginParams.AuthSalt))
        {
            throw new InvalidOperationException("User not found or error fetching login parameters.");
        }

        int iterations = loginParams.KdfParams?.Iterations ?? CryptoUtils.DefaultKdfIterations;
        int memorySize = loginParams.KdfParams?.MemorySize ?? CryptoUtils.DefaultKdfMemorySize;
        int parallelism = loginParams.KdfParams?.Parallelism ?? CryptoUtils.DefaultKdfParallelism;

        var authSaltBytes = Convert.FromBase64String(loginParams.AuthSalt);
        var secretKeyBytes = Convert.FromBase64String(secretKeyBase64.Trim());

        var masterKey = CryptoUtils.DeriveMasterKey(password, secretKeyBytes, authSaltBytes, iterations, memorySize, parallelism);
        var authHash = CryptoUtils.ComputeAuthHash(masterKey);

        var request = new LoginRequest
        {
            Email = email,
            AuthHash = authHash
        };

        var authResponse = await _apiService.LoginAsync(request);
        if (authResponse == null)
        {
            throw new InvalidOperationException("Invalid credentials.");
        }

        byte[]? userPrivateKey = null;
        if (!string.IsNullOrEmpty(authResponse.EncryptedUserPrivateKey))
        {
            var encPrivKey = Convert.FromBase64String(authResponse.EncryptedUserPrivateKey);
            userPrivateKey = CryptoUtils.AesGcmDecrypt(masterKey, encPrivKey);
        }

        byte[]? userPublicKey = null;
        if (!string.IsNullOrEmpty(authResponse.UserPublicKey))
        {
            userPublicKey = Convert.FromBase64String(authResponse.UserPublicKey);
        }

        // Save encrypted Secret Key for fast session unlock with only Master Password
        var encryptedSecretKey = CryptoUtils.EncryptSecretKeyForSession(secretKeyBytes, password, authSaltBytes);
        var sessionLock = new SessionLockInfo
        {
            Email = email,
            AuthSaltBase64 = loginParams.AuthSalt,
            EncryptedSecretKeyBase64 = Convert.ToBase64String(encryptedSecretKey),
            EncryptedUserPrivateKeyBase64 = authResponse.EncryptedUserPrivateKey,
            Iterations = iterations,
            MemorySize = memorySize,
            Parallelism = parallelism,
            IsLocked = false
        };
        await _sessionLockStorage.SetSessionLockInfoAsync(sessionLock);

        return new LoginResult(authResponse, userPrivateKey, userPublicKey);
    }

    public async Task<LoginResult> UnlockWithMasterPasswordAsync(string password)
    {
        var sessionLock = await _sessionLockStorage.GetSessionLockInfoAsync();
        if (sessionLock == null || string.IsNullOrWhiteSpace(sessionLock.EncryptedSecretKeyBase64))
        {
            throw new InvalidOperationException("No locked session found. Please sign in with your Secret Key.");
        }

        var authSaltBytes = Convert.FromBase64String(sessionLock.AuthSaltBase64);
        var encryptedSecretKey = Convert.FromBase64String(sessionLock.EncryptedSecretKeyBase64);

        byte[] secretKeyBytes;
        try
        {
            secretKeyBytes = CryptoUtils.DecryptSecretKeyFromSession(encryptedSecretKey, password, authSaltBytes);
        }
        catch
        {
            throw new InvalidOperationException("Invalid Master Password.");
        }

        var secretKeyBase64 = Convert.ToBase64String(secretKeyBytes);
        return await LoginAsync(sessionLock.Email, password, secretKeyBase64);
    }
}
