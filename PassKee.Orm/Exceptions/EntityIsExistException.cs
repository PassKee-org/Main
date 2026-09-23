namespace PassKee.Orm.Exceptions
{
    public class EntityIsExistException: Exception
    {
        public EntityIsExistException(string name = "") : base($"Record is exist. {name}")
        {
        }
    }
}