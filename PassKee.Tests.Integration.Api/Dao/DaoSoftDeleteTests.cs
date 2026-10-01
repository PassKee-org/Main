using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NHibernate.Linq;
using PassKee.Business.Common.Constants;
using PassKee.Orm.Dao;
using PassKee.Orm.Dao.Vaults;
using PassKee.Orm.Entities;
using PassKee.Orm.Entities.Vaults;
using PassKee.Tests.Integration.Api.Core;
using Xunit;

namespace PassKee.Tests.Integration.Api.Dao;

public class DaoSoftDeleteTests : BaseTest
{
    private readonly IUserDao _userDao;
    private readonly IUserAccessTokenDao _accessTokenDao;
    private readonly IVaultDao _vaultDao;
    private readonly IDirectoryDao _directoryDao;
    private readonly ICredentialDao _credentialDao;

    public DaoSoftDeleteTests(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        _userDao = ServiceProvider.GetRequiredService<IUserDao>();
        _accessTokenDao = ServiceProvider.GetRequiredService<IUserAccessTokenDao>();
        _vaultDao = ServiceProvider.GetRequiredService<IVaultDao>();
        _directoryDao = ServiceProvider.GetRequiredService<IDirectoryDao>();
        _credentialDao = ServiceProvider.GetRequiredService<ICredentialDao>();
    }

    [Fact]
    public async Task UserDao_ShouldFilterSoftDeletedUser()
    {
        var user = await UserSeeder.CreateAsync();
        await FlushDbChanges();

        var activeUserById = await _userDao.GetById(user.Id);
        var activeUserByEmail = await _userDao.GetByEmail(user.Email);
        Assert.NotNull(activeUserById);
        Assert.NotNull(activeUserByEmail);

        await _userDao.DeleteAsync(user);
        await FlushDbChanges();

        var deletedUserById = await _userDao.GetById(user.Id);
        var deletedUserByEmail = await _userDao.GetByEmail(user.Email);
        Assert.Null(deletedUserById);
        Assert.Null(deletedUserByEmail);
    }

    [Fact]
    public async Task UserAccessTokenDao_ShouldFilterSoftDeletedTokenAndUser()
    {
        var user = await UserSeeder.CreateAsync();
        var token = await _accessTokenDao.CreateNew(user);
        await FlushDbChanges();

        var activeToken = await _accessTokenDao.GetByToken(token.Token);
        Assert.NotNull(activeToken);

        // 1. Soft-delete the token using base DeleteAsync
        await _accessTokenDao.DeleteAsync(token);
        await FlushDbChanges();

        Assert.Null(await _accessTokenDao.GetByToken(token.Token));
        Assert.Null(await _accessTokenDao.GetById(token.Id));

        // 2. Restore token, but soft-delete the user
        token.DeletedAt = null;
        await _userDao.DeleteAsync(user);
        await DbSessionProvider.CurrentSession.UpdateAsync(token);
        await FlushDbChanges();

        Assert.Null(await _accessTokenDao.GetByToken(token.Token));
        Assert.Null(await _accessTokenDao.GetById(token.Id));
    }

    [Fact]
    public async Task UserAccessTokenDao_Delete_ShouldSoftDeleteTokenAndJwtTokens()
    {
        var user = await UserSeeder.CreateAsync();
        var token = await _accessTokenDao.CreateNew(user);
        var jwtTokenEntity = new UserJwtTokenEntity
        {
            Token = "test_jwt_token_12345",
            CreatedAt = DateTime.UtcNow,
            ExpirationTime = DateTime.UtcNow.AddHours(1),
            AccessToken = token
        };
        await DbSessionProvider.CurrentSession.SaveAsync(jwtTokenEntity);
        await FlushDbChanges();

        var hasJwtBefore = await _accessTokenDao.HasJwtToken(token, jwtTokenEntity.Token);
        Assert.True(hasJwtBefore);

        await _accessTokenDao.Delete(token);
        await FlushDbChanges();

        Assert.NotNull(token.DeletedAt);
        Assert.Null(await _accessTokenDao.GetByToken(token.Token));
        Assert.Null(await _accessTokenDao.GetById(token.Id));

        var hasJwtAfter = await _accessTokenDao.HasJwtToken(token, jwtTokenEntity.Token);
        Assert.False(hasJwtAfter);
    }

    [Fact]
    public async Task VaultDao_ShouldFilterSoftDeletedVault()
    {
        var user = await UserSeeder.CreateAsync();
        var vault = new VaultEntity
        {
            UserId = user.Id,
            Name = "Secret Vault",
            EncryptedVaultKey = new byte[] { 1, 2, 3 }
        };
        await DbSessionProvider.CurrentSession.SaveAsync(vault);
        await FlushDbChanges();

        var activeVault = await _vaultDao.GetById(vault.Id);
        var activeVaults = await _vaultDao.GetByUserId(user.Id);
        Assert.NotNull(activeVault);
        Assert.Single(activeVaults);

        await _vaultDao.DeleteAsync(vault);
        await FlushDbChanges();

        var deletedVault = await _vaultDao.GetById(vault.Id);
        var deletedVaults = await _vaultDao.GetByUserId(user.Id);
        Assert.Null(deletedVault);
        Assert.Empty(deletedVaults);
    }

    [Fact]
    public async Task DirectoryDao_ShouldFilterSoftDeletedDirectory()
    {
        var user = await UserSeeder.CreateAsync();
        var vault = new VaultEntity
        {
            UserId = user.Id,
            Name = "Vault",
            EncryptedVaultKey = new byte[] { 1 }
        };
        await DbSessionProvider.CurrentSession.SaveAsync(vault);

        var dir = new DirectoryEntity
        {
            VaultId = vault.Id,
            EncryptedName = new byte[] { 2 }
        };
        await DbSessionProvider.CurrentSession.SaveAsync(dir);
        await FlushDbChanges();

        var activeDir = await _directoryDao.GetById(dir.Id);
        var activeDirs = await _directoryDao.GetByVaultId(vault.Id);
        Assert.NotNull(activeDir);
        Assert.Single(activeDirs);

        await _directoryDao.DeleteAsync(dir);
        await FlushDbChanges();

        var deletedDir = await _directoryDao.GetById(dir.Id);
        var deletedDirs = await _directoryDao.GetByVaultId(vault.Id);
        Assert.Null(deletedDir);
        Assert.Empty(deletedDirs);
    }

    [Fact]
    public async Task CredentialDao_ShouldFilterSoftDeletedCredential()
    {
        var user = await UserSeeder.CreateAsync();
        var vault = new VaultEntity
        {
            UserId = user.Id,
            Name = "Vault",
            EncryptedVaultKey = new byte[] { 1 }
        };
        await DbSessionProvider.CurrentSession.SaveAsync(vault);

        var dir = new DirectoryEntity
        {
            VaultId = vault.Id,
            EncryptedName = new byte[] { 2 }
        };
        await DbSessionProvider.CurrentSession.SaveAsync(dir);

        var cred = new CredentialEntity
        {
            VaultId = vault.Id,
            DirectoryId = dir.Id,
            Type = CredentialType.Login,
            EncryptedBody = new byte[] { 3 }
        };
        await DbSessionProvider.CurrentSession.SaveAsync(cred);
        await FlushDbChanges();

        var activeCred = await _credentialDao.GetById(cred.Id);
        var activeCreds = await _credentialDao.GetByVaultId(vault.Id);
        var activeByDir = await _credentialDao.GetByDirectoryIds(vault.Id, new[] { dir.Id });
        Assert.NotNull(activeCred);
        Assert.Single(activeCreds);
        Assert.Single(activeByDir);

        await _credentialDao.DeleteAsync(cred);
        await FlushDbChanges();

        var deletedCred = await _credentialDao.GetById(cred.Id);
        var deletedCreds = await _credentialDao.GetByVaultId(vault.Id);
        var deletedByDir = await _credentialDao.GetByDirectoryIds(vault.Id, new[] { dir.Id });
        Assert.Null(deletedCred);
        Assert.Empty(deletedCreds);
        Assert.Empty(deletedByDir);
    }

    [Fact]
    public async Task BaseDao_DeleteAsync_ShouldSoftDeleteBatch()
    {
        var user = await UserSeeder.CreateAsync();
        var vault = new VaultEntity
        {
            UserId = user.Id,
            Name = "Vault",
            EncryptedVaultKey = new byte[] { 1 }
        };
        await DbSessionProvider.CurrentSession.SaveAsync(vault);

        var creds = new List<CredentialEntity>();
        for (int i = 0; i < 3; i++)
        {
            var cred = new CredentialEntity
            {
                VaultId = vault.Id,
                Type = CredentialType.SecureNote,
                EncryptedBody = new byte[] { (byte)i }
            };
            await DbSessionProvider.CurrentSession.SaveAsync(cred);
            creds.Add(cred);
        }
        await FlushDbChanges();

        var activeBefore = await _credentialDao.GetByVaultId(vault.Id);
        Assert.Equal(3, activeBefore.Count);

        await _credentialDao.DeleteAsync(creds);
        await FlushDbChanges();

        var activeAfter = await _credentialDao.GetByVaultId(vault.Id);
        Assert.Empty(activeAfter);

        foreach (var c in creds)
        {
            Assert.NotNull(c.DeletedAt);
            Assert.True(c.IsDeleted);
        }
    }

    [Fact]
    public async Task QueueDao_ShouldFilterSoftDeletedQueueItem()
    {
        await _queueDao.Push(new { Test = 123 });
        await _queueDao.Flush();

        var queueItem = await DbSessionProvider.CurrentSession.Query<QueueEntity>().FirstOrDefaultAsync();
        Assert.NotNull(queueItem);

        var activeById = await _queueDao.GetById(queueItem.Id);
        Assert.NotNull(activeById);

        queueItem.DeletedAt = DateTime.UtcNow;
        await DbSessionProvider.CurrentSession.UpdateAsync(queueItem);
        await FlushDbChanges();

        var deletedById = await _queueDao.GetById(queueItem.Id);
        Assert.Null(deletedById);
    }
}
