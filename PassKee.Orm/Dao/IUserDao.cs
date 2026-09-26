using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.Abstractions;
using PassKee.Orm.Entities;

namespace PassKee.Orm.Dao;

public interface IUserDao : IDomainService
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
        string? kdfParams = null,
        CancellationToken cancellationToken = default
    );
}
