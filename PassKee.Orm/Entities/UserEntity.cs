using PassKee.Orm.Core;

namespace PassKee.Orm.Entities;

public class UserEntity : AEntity
{
    public virtual string Email { get; set; } = null!;
}
