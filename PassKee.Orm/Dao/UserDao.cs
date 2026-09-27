using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using NHibernate.Linq;
using PassKee.Business.Common.Utils;
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
        var normalizedEmail = email.Trim().ToLower();
        return await Session.Query<UserEntity>()
            .Where(x => x.Email.ToLower() == normalizedEmail)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<UserEntity> Create(
        string email,
        byte[] authSalt,
        byte[] serverHash,
        byte[] userPublicKey,
        byte[] encryptedUserPrivateKey,
        byte[] encryptedUserVaultKey,
        KdfParameters? kdfParams = null,
        CancellationToken cancellationToken = default
    )
    {
        var user = new UserEntity
        {
            Email = email.Trim().ToLower(),
            AuthSalt = authSalt,
            ServerHash = serverHash,
            UserPublicKey = userPublicKey,
            EncryptedUserPrivateKey = encryptedUserPrivateKey,
            EncryptedUserVaultKey = encryptedUserVaultKey,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await Session.SaveAsync(user, cancellationToken);

        kdfParams ??= new KdfParameters(
            CryptoUtils.DefaultKdfIterations,
            CryptoUtils.DefaultKdfMemorySize,
            CryptoUtils.DefaultKdfParallelism
        );

        var kdfEntity = new UserKdfParamsEntity
        {
            User = user,
            Iterations = kdfParams.Iterations,
            MemorySize = kdfParams.MemorySize,
            Parallelism = kdfParams.Parallelism,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await Session.SaveAsync(kdfEntity, cancellationToken);
        user.KdfParams = kdfEntity;

        return user;
    }
}
