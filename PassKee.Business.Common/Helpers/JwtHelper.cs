using Microsoft.IdentityModel.Tokens;

namespace PassKee.Business.Common.Helpers;

public static class JwtHelper
{
    private class JwtTokenModel
    {
        public long exp { get; set; }
    }
    
    public static DateTime GetExpiryTimestamp(string accessToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(accessToken))
                return DateTime.MinValue;
            if (!accessToken.Contains("."))
                return DateTime.MinValue;
 
            var parts = accessToken.Split('.');
            var payload = JsonHelper.DeserializeObject<JwtTokenModel>(Base64UrlEncoder.Decode(parts[1]));
            if (payload == null)
                throw new NullReferenceException(nameof(payload));
            var dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(payload.exp);
            return dateTimeOffset.UtcDateTime;
        }
        catch (Exception)
        {
            return DateTime.MinValue;
        }
    }
}
