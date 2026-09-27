using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using PassKee.Api.Shared.Dto.RequestsAndResponses.Auth;
using PassKee.Business.Common.Utils;
using PassKee.Web.Services.Http;

namespace PassKee.Web.Pages.Auth;

public partial class Login
{
    [Inject]
    private ApiService ApiService { get; set; } = null!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;

    [SupplyParameterFromQuery(Name = "email")]
    private string? QueryEmail { get; set; }

    private string Email { get; set; } = "";
    private string Password { get; set; } = "";
    private string SecretKeyBase64 { get; set; } = "";
    private bool IsProcessing { get; set; } = false;
    private string ErrorMessage { get; set; } = "";

    protected override void OnInitialized()
    {
        if (!string.IsNullOrWhiteSpace(QueryEmail) && string.IsNullOrWhiteSpace(Email))
        {
            Email = QueryEmail;
        }
    }

    private async Task LoginAsync()
    {
        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password) || string.IsNullOrWhiteSpace(SecretKeyBase64))
        {
            ErrorMessage = "All fields are required.";
            return;
        }

        IsProcessing = true;
        ErrorMessage = "";
        StateHasChanged();

        await Task.Delay(10); // yield to UI

        try
        {
            // 1. Fetch Login Parameters via ApiService
            var loginParams = await ApiService.GetLoginParamsAsync(Email);
            if (loginParams == null || string.IsNullOrEmpty(loginParams.AuthSalt))
            {
                ErrorMessage = "User not found or error fetching login parameters.";
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
            var secretKeyBytes = Convert.FromBase64String(SecretKeyBase64.Trim());

            // 3. Derive Master Key & Auth Hash using shared common helpers
            var masterKey = CryptoUtils.DeriveMasterKey(Password, secretKeyBytes, authSaltBytes, iterations, memorySize, parallelism);
            var authHash = CryptoUtils.ComputeAuthHash(masterKey);

            // 4. Submit Login via ApiService
            var request = new LoginRequest
            {
                Email = Email,
                AuthHash = authHash
            };

            var authResponse = await ApiService.LoginAsync(request);
            if (authResponse != null && !string.IsNullOrEmpty(authResponse.AccessToken))
            {
                NavigationManager.NavigateTo("/app");
            }
            else
            {
                ErrorMessage = "Invalid credentials.";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error during login: {ex.Message}";
        }
        finally
        {
            IsProcessing = false;
        }
    }
}

