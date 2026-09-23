using Domain.Abstractions;

namespace PassKee.Business.Common.Exceptions.Common
{
    public class DataInconsistencyException : Exception, IDomainException
    {
        public DataInconsistencyException(): this("")
        {
        }

        public DataInconsistencyException(string message) : base($"Data inconsistency. {message}")
        {
        }
    }
}
