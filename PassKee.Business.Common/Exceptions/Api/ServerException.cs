using Domain.Abstractions;

namespace PassKee.Business.Common.Exceptions.Api
{
    public class ServerException : Exception, IDomainException
    {
        public ServerException(string message = "Server error") : base(message)
        {
        }
    }
}
