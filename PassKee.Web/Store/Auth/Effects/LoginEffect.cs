using System;
using System.Text.Json;
using System.Threading.Tasks;
using Fluxor;
using PassKee.Api.Shared.Dto.RequestsAndResponses.Auth;
using PassKee.Business.Common.Utils;
using PassKee.Web.Services.Http;

namespace PassKee.Web.Store.Auth.Effects;

public class LoginEffect : Effect<LoginAction>
{
    private readonly ApiService _apiService;

    public LoginEffect(ApiService apiService)
    {
        _apiService = apiService;
    }

    public override async Task HandleAsync(LoginAction action, IDispatcher dispatcher)
    {
        await Task.Delay(10); // Yield to UI so loading spinner can render

        try
        {
            // 1. Fetch Login Parameters via ApiService
            var loginParams = await _apiService.GetLoginParamsAsync(action.Email);
            if (loginParams == null || string.IsNullOrEmpty(loginParams.AuthSalt))
            {
                dispatcher.Dispatch(new LoginFailureAction("User not found or error fetching login parameters."));
                return;
            }

            // 2. Parse KDF Parameters
            int iterations = CryptoUtils.DefaultKdfIterations;
            int memorySize = CryptoUtils.DefaultKdfMemorySize;
            int parallelism = CryptoUtils.DefaultKdfParallelism;

            if (!string.IsNullOrEmpty(loginParams.KdfParams))
            {
                using var kdfDoc = JsonDocument.Parse(loginParams.KdfParams);
                if (kdfDoc.RootElement.TryGetProperty("iterations", out var itProp))
                    iterations = itProp.GetInt32();
                if (kdfDoc.RootElement.TryGetProperty("memorySize", out var memProp))
                    memorySize = memProp.GetInt32();
                if (kdfDoc.RootElement.TryGetProperty("parallelism", out var parProp))
                    parallelism = parProp.GetInt32();
            }

            var authSaltBytes = Convert.FromBase64String(loginParams.AuthSalt);
            var secretKeyBytes = Convert.FromBase64String(action.SecretKeyBase64.Trim());

            // 3. Derive Master Key & Auth Hash using shared common helpers
            var masterKey = CryptoUtils.DeriveMasterKey(action.Password, secretKeyBytes, authSaltBytes, iterations, memorySize, parallelism);
            var authHash = CryptoUtils.ComputeAuthHash(masterKey);

            // 4. Submit Login via ApiService
            var request = new LoginRequest
            {
                Email = action.Email,
                AuthHash = authHash
            };

            var authResponse = await _apiService.LoginAsync(request);
            if (authResponse != null && !string.IsNullOrEmpty(authResponse.AccessToken))
            {
                dispatcher.Dispatch(new LoginSuccessAction(authResponse));
            }
            else
            {
                dispatcher.Dispatch(new LoginFailureAction("Invalid credentials."));
            }
        }
        catch (Exception ex)
        {
            dispatcher.Dispatch(new LoginFailureAction($"Error during login: {ex.Message}"));
        }
    }
}
