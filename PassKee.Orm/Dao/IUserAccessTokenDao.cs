using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.Abstractions;
using PassKee.Orm.Dao.Common;
using PassKee.Orm.Entities;

namespace PassKee.Orm.Dao;

public interface IUserAccessTokenDao : IBaseDao
{
    Task<UserAccessTokenEntity> CreateNew(UserEntity user, CancellationToken cancellationToken = default);

    Task<UserAccessTokenEntity?> GetByToken(string accessToken, CancellationToken cancellationToken = default);

    Task<UserAccessTokenEntity?> GetById(Guid id, CancellationToken cancellationToken = default);

    Task<bool> HasJwtToken(UserAccessTokenEntity accessToken, string jwtToken, CancellationToken cancellationToken = default);

    Task DeleteExpiredJwtTokens(UserAccessTokenEntity accessToken, CancellationToken cancellationToken = default);

    Task Delete(UserAccessTokenEntity accessToken, CancellationToken cancellationToken = default);
    Task<UserJwtTokenEntity> CreateJwtToken(UserAccessTokenEntity accessToken, string token, DateTime expirationTime, CancellationToken cancellationToken = default);
}
