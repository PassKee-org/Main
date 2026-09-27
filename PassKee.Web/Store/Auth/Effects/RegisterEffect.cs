using System;
using System.Threading.Tasks;
using Fluxor;
using PassKee.Api.Shared.Dto.RequestsAndResponses.Auth;
using PassKee.Business.Common.Utils;
using PassKee.Web.Services.Http;

namespace PassKee.Web.Store.Auth.Effects;

public class RegisterEffect : Effect<RegisterAction>
{
    private readonly ApiService _apiService;

    public RegisterEffect(ApiService apiService)
    {
        _apiService = apiService;
    }

    public override async Task HandleAsync(RegisterAction action, IDispatcher dispatcher)
    {
        await Task.Delay(10); // Yield to UI so loading spinner can render

        try
        {
            var regData = CryptoUtils.PrepareClientRegistration(action.Password);
            var secretKeyBase64 = Convert.ToBase64String(regData.SecretKey);

            var request = new RegisterRequest
            {
                Email = action.Email,
                AuthHash = regData.AuthHash,
                AuthSalt = regData.AuthSalt,
                KdfParams = regData.KdfParams,
                UserPublicKey = regData.KeyEnvelope.PublicKey,
                EncryptedUserPrivateKey = regData.KeyEnvelope.EncryptedPrivateKey,
                EncryptedUserVaultKey = regData.KeyEnvelope.EncryptedVaultKey
            };

            var response = await _apiService.RegisterAsync(request);
            if (response != null && !string.IsNullOrEmpty(response.AccessToken))
            {
                dispatcher.Dispatch(new RegisterSuccessAction(response, secretKeyBase64));
            }
            else
            {
                dispatcher.Dispatch(new RegisterFailureAction("Registration failed. Please try again."));
            }
        }
        catch (Exception ex)
        {
            dispatcher.Dispatch(new RegisterFailureAction($"Error during registration: {ex.Message}"));
        }
    }
}
