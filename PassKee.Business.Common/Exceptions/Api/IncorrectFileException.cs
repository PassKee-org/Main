using Domain.Abstractions;

namespace PassKee.Business.Common.Exceptions.Api
{
    public class IncorrectFileException : Exception, IDomainException
    {
        public IncorrectFileException(string message = "") : base(message)
        {
        }
    }
}
