using Domain.Abstractions;

namespace PassKee.Business.Common.Exceptions.Api
{
    public class RecordCanNotBeModifiedException : Exception, IDomainException
    {
        public RecordCanNotBeModifiedException(string message = "Record can not be modified") : base(message)
        {
        }
    }
}
