using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.Abstractions;
using PassKee.Business.Common.Utils;
using PassKee.Orm.Dao.Common;
using PassKee.Orm.Entities;

namespace PassKee.Orm.Dao;

public interface IUserDao : IBaseDao
{
    Task<UserEntity?> GetById(Guid id, CancellationToken cancellationToken = default);
    Task<UserEntity?> GetByEmail(string email, CancellationToken cancellationToken = default);
    Task<UserEntity> Create(
        string email,
        byte[] authSalt,
        byte[] serverHash,
        byte[] userPublicKey,
        byte[] encryptedUserPrivateKey,
        byte[] encryptedUserVaultKey,
        KdfParameters? kdfParams = null,
        CancellationToken cancellationToken = default
    );
}
