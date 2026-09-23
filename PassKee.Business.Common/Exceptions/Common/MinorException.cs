using Domain.Abstractions;

namespace PassKee.Business.Common.Exceptions.Common
{
    public class MinorException : Exception, IDomainException
    {
        public MinorException(): this("")
        {
        }

        public MinorException(string message) : base(message)
        {
        }
    }
}
