using Domain.Abstractions;

namespace PassKee.Orm.Exceptions
{
    public class EntityIsNotExistException: Exception, IDomainException
    {
        public EntityIsNotExistException(string name = "") : base($"Record does not exist. {name}")
        {
        }
    }
}
