using Domain.Abstractions;

namespace PassKee.Business.Common.Exceptions.Common
{
    public class ValidationException : Exception, IDomainException
    {
        public ValidationException(): this("")
        {
        }

        public ValidationException(string message) : base(message)
        {
        }
    }
}
