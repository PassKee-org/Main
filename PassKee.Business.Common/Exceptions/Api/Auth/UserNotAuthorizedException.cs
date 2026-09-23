using System.Security.Authentication;
using Domain.Abstractions;

namespace PassKee.Business.Common.Exceptions.Api.Auth
{
    public class UserNotAuthorizedException : AuthenticationException, IDomainException
    {

    }
}
