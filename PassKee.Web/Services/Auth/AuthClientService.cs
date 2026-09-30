using System;
using System.Security.Cryptography;
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

    public async Task<RegisterResult> RegisterAsync(string email, string password, string? secretKey = null)
    {
        secretKey = !string.IsNullOrWhiteSpace(secretKey) ? secretKey.Trim() : CryptoUtils.GenerateSecretKeyString();
        var regData = CryptoUtils.PrepareClientRegistration(password, secretKey);
        byte[]? userPrivateKey = null;
        var succeeded = false;

        try
        {
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

            userPrivateKey = CryptoUtils.AesGcmDecrypt(regData.MasterKey, regData.KeyEnvelope.EncryptedPrivateKey);

            var sessionUnlockSalt = CryptoUtils.GenerateRandomBytes(32);
            var encryptedSecretKey = CryptoUtils.EncryptSecretKeyForSession(regData.SecretKey, password, sessionUnlockSalt);
            var sessionLock = new SessionLockInfo
            {
                FormatVersion = SessionLockInfo.CurrentFormatVersion,
                Email = email,
                AuthSaltBase64 = Convert.ToBase64String(regData.AuthSalt),
                EncryptedSecretKeyBase64 = Convert.ToBase64String(encryptedSecretKey),
                SessionUnlockSaltBase64 = Convert.ToBase64String(sessionUnlockSalt),
                Iterations = regData.KdfParams.Iterations,
                MemorySize = regData.KdfParams.MemorySize,
                Parallelism = regData.KdfParams.Parallelism,
                IsLocked = false
            };
            await _sessionLockStorage.SetSessionLockInfoAsync(sessionLock);

            succeeded = true;
            return new RegisterResult(response, secretKey, userPrivateKey, regData.KeyEnvelope.PublicKey);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(regData.SecretKey);
            CryptographicOperations.ZeroMemory(regData.MasterKey);
            CryptographicOperations.ZeroMemory(regData.AuthHash);
            CryptographicOperations.ZeroMemory(regData.KeyEnvelope.PlainVaultKey);
            if (!succeeded && userPrivateKey != null)
            {
                CryptographicOperations.ZeroMemory(userPrivateKey);
            }
        }
    }

    public async Task<LoginResult> LoginAsync(string email, string password, string secretKey)
    {
        var secretKeyBytes = CryptoUtils.ParseSecretKey(secretKey);
        return await LoginAsync(email, password, secretKeyBytes);
    }

    public async Task<LoginResult> LoginAsync(string email, string password, byte[] secretKeyBytes)
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
        byte[]? masterKey = null;
        byte[]? authHash = null;
        byte[]? userPrivateKey = null;
        var succeeded = false;

        try
        {
            masterKey = CryptoUtils.DeriveMasterKey(password, secretKeyBytes, authSaltBytes, iterations, memorySize, parallelism);
            authHash = CryptoUtils.ComputeAuthHash(masterKey);

            var authResponse = await _apiService.LoginAsync(new LoginRequest
            {
                Email = email,
                AuthHash = authHash
            });
            if (authResponse == null)
            {
                throw new InvalidOperationException("Invalid credentials.");
            }

            if (!string.IsNullOrEmpty(authResponse.EncryptedUserPrivateKey))
            {
                var encryptedPrivateKey = Convert.FromBase64String(authResponse.EncryptedUserPrivateKey);
                userPrivateKey = CryptoUtils.AesGcmDecrypt(masterKey, encryptedPrivateKey);
            }

            byte[]? userPublicKey = null;
            if (!string.IsNullOrEmpty(authResponse.UserPublicKey))
            {
                userPublicKey = Convert.FromBase64String(authResponse.UserPublicKey);
            }

            var sessionUnlockSalt = CryptoUtils.GenerateRandomBytes(32);
            var encryptedSecretKey = CryptoUtils.EncryptSecretKeyForSession(secretKeyBytes, password, sessionUnlockSalt);
            var sessionLock = new SessionLockInfo
            {
                FormatVersion = SessionLockInfo.CurrentFormatVersion,
                Email = email,
                AuthSaltBase64 = loginParams.AuthSalt,
                EncryptedSecretKeyBase64 = Convert.ToBase64String(encryptedSecretKey),
                SessionUnlockSaltBase64 = Convert.ToBase64String(sessionUnlockSalt),
                Iterations = iterations,
                MemorySize = memorySize,
                Parallelism = parallelism,
                IsLocked = false
            };
            await _sessionLockStorage.SetSessionLockInfoAsync(sessionLock);

            succeeded = true;
            return new LoginResult(authResponse, userPrivateKey, userPublicKey);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(secretKeyBytes);
            if (masterKey != null)
            {
                CryptographicOperations.ZeroMemory(masterKey);
            }
            if (authHash != null)
            {
                CryptographicOperations.ZeroMemory(authHash);
            }
            if (!succeeded && userPrivateKey != null)
            {
                CryptographicOperations.ZeroMemory(userPrivateKey);
            }
        }
    }

    public async Task<LoginResult> UnlockWithMasterPasswordAsync(string password)
    {
        var sessionLock = await _sessionLockStorage.GetSessionLockInfoAsync();
        if (sessionLock == null || string.IsNullOrWhiteSpace(sessionLock.EncryptedSecretKeyBase64))
        {
            throw new InvalidOperationException("No locked session found. Please sign in with your Secret Key.");
        }
        if (sessionLock.FormatVersion != SessionLockInfo.CurrentFormatVersion || string.IsNullOrWhiteSpace(sessionLock.SessionUnlockSaltBase64))
        {
            throw new InvalidOperationException("This saved session uses an outdated format. Switch account and sign in with your Secret Key.");
        }

        var sessionUnlockSalt = Convert.FromBase64String(sessionLock.SessionUnlockSaltBase64);
        var encryptedSecretKey = Convert.FromBase64String(sessionLock.EncryptedSecretKeyBase64);

        byte[] secretKeyBytes;
        try
        {
            secretKeyBytes = CryptoUtils.DecryptSecretKeyFromSession(encryptedSecretKey, password, sessionUnlockSalt);
        }
        catch
        {
            throw new InvalidOperationException("Invalid Master Password.");
        }

        try
        {
            return await LoginAsync(sessionLock.Email, password, secretKeyBytes);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(secretKeyBytes);
        }
    }
}
