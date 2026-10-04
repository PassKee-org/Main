using System;
using System.Threading.Tasks;
using PassKee.Business.Common.Constants;
using PassKee.Business.Common.Exceptions.Api;
using PassKee.Orm.Core;
using PassKee.Orm.Dao;
using PassKee.Orm.Entities;
using PassKee.Orm.Entities.Vaults;
using Persistence.Transactions.Behaviors;

namespace PassKee.Api.Services.Security;

public class SecurityService : ISecurityService
{
    private readonly IDbSessionProvider _sessionProvider;

    public SecurityService(IDbSessionProvider sessionProvider)
    {
        _sessionProvider = sessionProvider;
    }

    public async Task CheckAccess<TEntity>(AccessLevel accessLevel, UserEntity user, TEntity? entity)
    {
        if (entity == null || (entity is AEntity { IsDeleted: true }))
        {
            throw new RecordNotFoundException();
        }

        if (user == null || user.IsDeleted)
        {
            throw new HasNoAccessException();
        }

        if (!await HasAccess(accessLevel, user, entity))
        {
            throw new HasNoAccessException();
        }
    }

    public async Task CheckAccess<TEntity>(AccessLevel accessLevel, Guid userId, TEntity? entity)
    {
        if (entity == null || (entity is AEntity { IsDeleted: true }))
        {
            throw new RecordNotFoundException();
        }

        if (!await HasAccess(accessLevel, userId, entity))
        {
            throw new HasNoAccessException();
        }
    }

    public Task<bool> HasAccess<TEntity>(AccessLevel accessLevel, UserEntity user, TEntity? entity)
    {
        if (user == null || user.IsDeleted || entity == null || (entity is AEntity { IsDeleted: true }))
        {
            return Task.FromResult(false);
        }

        return HasAccess(accessLevel, user.Id, entity);
    }

    public async Task<bool> HasAccess<TEntity>(AccessLevel accessLevel, Guid userId, TEntity? entity)
    {
        if (entity == null || (entity is AEntity { IsDeleted: true }))
        {
            return false;
        }

        if (entity is VaultEntity vaultEntity)
        {
            return HasAccessToVault(accessLevel, userId, vaultEntity);
        }

        if (entity is DirectoryEntity directoryEntity)
        {
            return await HasAccessToDirectory(accessLevel, userId, directoryEntity);
        }

        if (entity is CredentialEntity credentialEntity)
        {
            return await HasAccessToCredential(accessLevel, userId, credentialEntity);
        }

        if (entity is TagEntity tagEntity)
        {
            return await HasAccessToTag(accessLevel, userId, tagEntity);
        }

        throw new NotImplementedException($"Security checking not implemented for {entity.GetType().Name}");
    }

    private async Task<bool> HasAccessToTag(AccessLevel accessLevel, Guid userId, TagEntity tag)
    {
        if (tag.DeletedAt != null)
        {
            return false;
        }

        var vault = await _sessionProvider.CurrentSession.GetAsync<VaultEntity>(tag.VaultId);
        return vault != null && HasAccessToVault(accessLevel, userId, vault);
    }

    private static bool HasAccessToVault(AccessLevel accessLevel, Guid userId, VaultEntity vault)
    {
        return vault.UserId == userId && vault.DeletedAt == null;
    }

    private async Task<bool> HasAccessToDirectory(AccessLevel accessLevel, Guid userId, DirectoryEntity directory)
    {
        if (directory.DeletedAt != null)
        {
            return false;
        }

        var vault = await _sessionProvider.CurrentSession.GetAsync<VaultEntity>(directory.VaultId);
        return vault != null && HasAccessToVault(accessLevel, userId, vault);
    }

    private async Task<bool> HasAccessToCredential(AccessLevel accessLevel, Guid userId, CredentialEntity credential)
    {
        if (credential.DeletedAt != null)
        {
            return false;
        }

        var vault = await _sessionProvider.CurrentSession.GetAsync<VaultEntity>(credential.VaultId);
        return vault != null && HasAccessToVault(accessLevel, userId, vault);
    }
}
