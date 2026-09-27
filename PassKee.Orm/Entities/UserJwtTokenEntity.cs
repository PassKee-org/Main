using System;
using PassKee.Orm.Core;

namespace PassKee.Orm.Entities;

public class UserJwtTokenEntity : AEntity
{
    public virtual required string Token { get; set; }
    public virtual DateTime ExpirationTime { get; set; }
    public virtual required UserAccessTokenEntity AccessToken { get; set; }

    #region Calculated

    public virtual bool IsExpired => ExpirationTime < DateTime.UtcNow;

    #endregion
}
