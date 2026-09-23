using Domain.Abstractions;

namespace PassKee.Orm.Exceptions
{
    public class DataInconsistentException: Exception, IDomainException
    {
        public DataInconsistentException(string error = $"Data is inconsistent") : base(error)
        {
        }
    }
}
