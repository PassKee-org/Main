using System.Security.Authentication;

namespace PassKee.Business.Common.Exceptions.Api.Auth;

public class IncorrectAccessTokenException: AuthenticationException
{
    public IncorrectAccessTokenException(
        string message = "Incorrect access token"
    ) : base(message)
    {
    }
}
