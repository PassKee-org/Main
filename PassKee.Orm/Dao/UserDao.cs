using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using NHibernate;
using NHibernate.Linq;
using PassKee.Orm.Dao.Common;
using PassKee.Orm.Entities;

namespace PassKee.Orm.Dao;

public class UserDao : BaseDao, IUserDao
{
    public UserDao(ILifetimeScope scope) : base(scope)
    {
    }

    public async Task<UserEntity?> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        return await Session.Query<UserEntity>()
            .Where(x => x.Id == id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<UserEntity?> GetByEmail(string email, CancellationToken cancellationToken = default)
    {
        return await Session.Query<UserEntity>()
            .Where(x => x.Email.ToLower() == email.ToLower())
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<UserEntity> Create(string email, CancellationToken cancellationToken = default)
    {
        var user = new UserEntity
        {
            Email = email,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await Session.SaveAsync(user, cancellationToken);
        return user;
    }
}

