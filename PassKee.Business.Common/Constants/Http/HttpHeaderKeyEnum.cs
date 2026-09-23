namespace PassKee.Business.Common.Constants.Http;

public enum HttpHeaderKeyEnum
{
    /// <summary>
    /// JWT token header parameter key
    /// </summary>
    JwtToken = 1,
    
    /// <summary>
    /// JWT token header parameter key
    /// </summary>
    AccessToken,
}


public static class HttpHeaderKeyEnumExtensions
{
    public static string GetKey(this HttpHeaderKeyEnum type)
    {
        switch (type)
        {
            case HttpHeaderKeyEnum.JwtToken:
                return "Tv-JWT-Token";
            case HttpHeaderKeyEnum.AccessToken:
                return "Tv-Access-Token";
        }

        throw new NotImplementedException($"CookieKey was not provided for such type: {type}");
    }
}
