using Domain.Abstractions;

namespace PassKee.Business.Common.Exceptions.Api
{
    public class HasNoAccessException : Exception, IDomainException
    {
        public HasNoAccessException(string message = "") : base(message)
        {
        }
    }
}
