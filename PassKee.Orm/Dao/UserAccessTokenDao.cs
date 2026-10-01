using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Extensions.Configuration;
using NHibernate.Linq;
using PassKee.Business.Common.Utils;
using PassKee.Orm.Dao.Common;
using PassKee.Orm.Entities;

namespace PassKee.Orm.Dao;

public class UserAccessTokenDao : BaseDao, IUserAccessTokenDao
{
    private readonly TimeSpan _expirationTimeout;

    public UserAccessTokenDao(
        ILifetimeScope scope,
        IConfiguration configuration
    ) : base(scope)
    {
        _expirationTimeout = TimeSpan.FromDays(configuration.GetValue<int>("App:Auth:AccessTokenLifetime", 30));
    }

    public async Task<UserAccessTokenEntity> CreateNew(UserEntity user, CancellationToken cancellationToken = default)
    {
        var accessToken = new UserAccessTokenEntity
        {
            User = user,
            Token = SecurityUtil.GetRandomString(64),
            CreatedAt = DateTime.UtcNow,
            ExpirationTime = DateTime.UtcNow + _expirationTimeout
        };
        await Session.SaveAsync(accessToken, cancellationToken);
        return accessToken;
    }

    public async Task<UserAccessTokenEntity?> GetByToken(string accessToken, CancellationToken cancellationToken = default)
    {
        return await Session.Query<UserAccessTokenEntity>()
            .Where(item => item.Token == accessToken && item.DeletedAt == null && item.User.DeletedAt == null)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<UserAccessTokenEntity?> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        return await Session.Query<UserAccessTokenEntity>()
            .Where(item => item.Id == id && item.DeletedAt == null && item.User.DeletedAt == null)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> HasJwtToken(UserAccessTokenEntity accessToken, string jwtToken, CancellationToken cancellationToken = default)
    {
        return await Session.Query<UserJwtTokenEntity>()
            .Where(item => item.AccessToken.Id == accessToken.Id && item.DeletedAt == null)
            .Where(item => item.Token == jwtToken)
            .AnyAsync(cancellationToken);
    }

    public async Task DeleteExpiredJwtTokens(UserAccessTokenEntity accessToken, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        await Session.Query<UserJwtTokenEntity>()
            .Where(item => item.AccessToken.Id == accessToken.Id && item.DeletedAt == null)
            .Where(item => item.ExpirationTime < now)
            .UpdateBuilder()
            .Set(x => x.DeletedAt, now)
            .UpdateAsync(cancellationToken);
    }

    public async Task Delete(UserAccessTokenEntity accessToken, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        await Session.Query<UserJwtTokenEntity>()
            .Where(item => item.AccessToken.Id == accessToken.Id && item.DeletedAt == null)
            .UpdateBuilder()
            .Set(x => x.DeletedAt, now)
            .UpdateAsync(cancellationToken);

        await DeleteAsync(accessToken, cancellationToken);
    }

    public async Task<UserJwtTokenEntity> CreateJwtToken(UserAccessTokenEntity accessToken, string token, DateTime expirationTime, CancellationToken cancellationToken = default)
    {
        var jwtTokenEntity = new UserJwtTokenEntity
        {
            Token = token,
            CreatedAt = DateTime.UtcNow,
            ExpirationTime = expirationTime,
            AccessToken = accessToken
        };
        accessToken.JwtTokens.Add(jwtTokenEntity);
        await Session.SaveAsync(jwtTokenEntity, cancellationToken);
        return jwtTokenEntity;
    }
}
