using Domain.Abstractions;

namespace PassKee.Business.Common.Exceptions.Api
{
    public class RecordIsExistsException : Exception, IDomainException
    {
        public RecordIsExistsException(string message = "Record is exists") : base(message)
        {
        }
    }
}
