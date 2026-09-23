using Domain.Abstractions;

namespace PassKee.Business.Common.Exceptions.Api
{
    public class TooManyRecordsException : Exception, IDomainException
    {
        public TooManyRecordsException(string message = "Too many records") : base(message)
        {
        }
    }
}
