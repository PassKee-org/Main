using Newtonsoft.Json;

namespace PassKee.Business.Common.Services.Web.ReCaptcha.Models;

public class ReCaptchaResponseModel
{
    [JsonProperty(PropertyName = "success")]
    public bool IsSuccess { get; set; }
        
    [JsonProperty(PropertyName = "challenge_ts")]
    public string ChallengeTime { get; set; }
        
    [JsonProperty(PropertyName = "hostname")]
    public string Hostname { get; set; }
        
    [JsonProperty(PropertyName = "error-codes")]
    public ICollection<string> ErrorCodes { get; set; }
}
