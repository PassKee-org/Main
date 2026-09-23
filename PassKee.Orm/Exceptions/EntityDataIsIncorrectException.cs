using Domain.Abstractions;

namespace PassKee.Orm.Exceptions
{
    public class EntityDataIsIncorrectException: Exception, IDomainException
    {
        public EntityDataIsIncorrectException(string name = "") : base($"Entity data is incorrect. {name}")
        {
        }
    }
}
