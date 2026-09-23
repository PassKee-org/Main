using System.Security.Authentication;

namespace PassKee.Business.Common.Exceptions.Api.Auth;

public class InvalidTokenException: AuthenticationException
{
    public InvalidTokenException(
        string message = "Invalid token"
    ) : base(message)
    {
    }
}
