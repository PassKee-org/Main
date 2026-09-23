using System.Net;
using System.Web;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PassKee.Business.Common.Services.Web.ReCaptcha.Models;

namespace PassKee.Business.Common.Services.Web.ReCaptcha;

public class ReCaptchaService: IReCaptchaService
{
    private readonly string _validateUrl = "https://www.google.com/recaptcha/api/siteverify";
    private static readonly HttpClient _httpClient = new();
    
    private readonly IConfiguration _configuration;
    private readonly ILogger<ReCaptchaService> _logger;
    private readonly string? _secretKey;

    public ReCaptchaService(
        IConfiguration configuration, 
        ILogger<ReCaptchaService> logger
    )
    {
        _configuration = configuration;
        _logger = logger;
        _secretKey = _configuration.GetValue<string?>("ReCaptcha:Secret");
    }
    
    public async Task<bool> ValidateAsync(string token)
    {
        try
        {
            var secretKey = HttpUtility.UrlEncode(_secretKey ?? string.Empty);
            var responseToken = HttpUtility.UrlEncode(token);
            var response = await _httpClient.GetAsync(
                $"{_validateUrl}?secret={secretKey}&response={responseToken}"
            );

            var responseDataString = await response.Content.ReadAsStringAsync();
            if (response.StatusCode == HttpStatusCode.OK)
            {
                try
                {
                    var responseModel = JsonConvert.DeserializeObject<ReCaptchaResponseModel>(responseDataString);
                    if (responseModel == null)
                    {
                        return false;
                    }
                    if (responseModel.IsSuccess)
                    {
                        return true;
                    }

                    var errorCodesString = string.Concat(responseModel.ErrorCodes, ", ");
                    _logger.LogDebug($"ReCaptcha API returned error response: \n{JsonConvert.SerializeObject(responseModel)}\nToken: {token}\nErrorCodes: {errorCodesString}\n                        ");
                }
                catch (Exception e)
                {
                    _logger.LogError(e.Message, e);
                }
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message, e);
        }
        return false;    
    }

    public bool HasSecretKey()
    {
        return !string.IsNullOrEmpty(_secretKey);
    }
}
