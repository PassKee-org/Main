using PassKee.Orm.Core;

namespace PassKee.Orm.Entities;

/// <summary>
/// Stores client KDF parameters (Argon2id) used to derive the Master Key for a specific user.
/// </summary>
public class UserKdfParamsEntity : AEntity
{
    public virtual UserEntity User { get; set; } = null!;

    public virtual int Iterations { get; set; }

    public virtual int MemorySize { get; set; }

    public virtual int Parallelism { get; set; }
}
