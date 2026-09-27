namespace PassKee.Business.Common.Constants.Http;

public enum HttpCookieKeyEnum
{
    /// <summary>
    /// JWT token cookie parameter key
    /// </summary>
    JwtToken = 1,
    
    /// <summary>
    /// JWT token cookie parameter key
    /// </summary>
    AccessToken,
}

public static class CookieKeyEnumExtensions
{
    public static string GetKey(this HttpCookieKeyEnum type)
    {
        switch (type)
        {
            case HttpCookieKeyEnum.JwtToken:
                return "pk_jwt_token";
            case HttpCookieKeyEnum.AccessToken:
                return "pk_access_token";
        }

        throw new NotImplementedException($"CookieKey was not provided for such type: {type}");
    }
}
