using Domain.Abstractions;
using PassKee.Business.Common.Resources;

namespace PassKee.Business.Common.Exceptions.Common
{
    public class PermissionException : Exception, IDomainException
    {
        public PermissionException(): this(RG.Error_UserHasNotPermissions)
        {
        }

        public PermissionException(string message) : base(message)
        {
        }
    }
}
