using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using PassKee.Api.Services.Security;
using PassKee.Business.Common.Constants;
using PassKee.Business.Common.Exceptions.Api;
using PassKee.Orm.Dao.Vaults;
using PassKee.Orm.Entities.Vaults;
using PassKee.Tests.Integration.Api.Core;
using Xunit;

namespace PassKee.Tests.Integration.Api.Services.Security.SecurityService;

public class CheckAccessTest : BaseTest
{
    private readonly ISecurityService _securityService;
    private readonly IVaultDao _vaultDao;

    public CheckAccessTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        _securityService = ServiceProvider.GetRequiredService<ISecurityService>();
        _vaultDao = ServiceProvider.GetRequiredService<IVaultDao>();
    }

    [Fact]
    public async Task ShouldThrowRecordNotFoundIfEntityIsNull()
    {
        var (_, owner) = await UserSeeder.CreateAuthorizedAsync();
        VaultEntity? nullVault = null;

        await Assert.ThrowsAsync<RecordNotFoundException>(() =>
            _securityService.CheckAccess(AccessLevel.Read, owner, nullVault));

        await Assert.ThrowsAsync<RecordNotFoundException>(() =>
            _securityService.CheckAccess(AccessLevel.Read, owner.Id, nullVault));
    }

    [Fact]
    public async Task ShouldThrowRecordNotFoundIfEntityIsSoftDeleted()
    {
        var (_, owner) = await UserSeeder.CreateAuthorizedAsync();
        var vault = await _vaultDao.CreateAsync(owner.Id, "Deleted Vault", new byte[] { 1, 2, 3 });
        await _vaultDao.DeleteAsync(vault);
        await FlushDbChanges();

        await Assert.ThrowsAsync<RecordNotFoundException>(() =>
            _securityService.CheckAccess(AccessLevel.Read, owner, vault));

        await Assert.ThrowsAsync<RecordNotFoundException>(() =>
            _securityService.CheckAccess(AccessLevel.Read, owner.Id, vault));
    }

    [Fact]
    public async Task ShouldThrowHasNoAccessIfUserDoesNotHaveAccess()
    {
        var (_, owner) = await UserSeeder.CreateAuthorizedAsync();
        var (_, otherUser) = await UserSeeder.CreateAuthorizedAsync();
        var vault = await _vaultDao.CreateAsync(owner.Id, "Owner Vault", new byte[] { 1, 2, 3 });
        await FlushDbChanges();

        await Assert.ThrowsAsync<HasNoAccessException>(() =>
            _securityService.CheckAccess(AccessLevel.Read, otherUser, vault));

        await Assert.ThrowsAsync<HasNoAccessException>(() =>
            _securityService.CheckAccess(AccessLevel.Read, otherUser.Id, vault));
    }

    [Fact]
    public async Task ShouldThrowHasNoAccessIfUserIsSoftDeleted()
    {
        var (_, owner) = await UserSeeder.CreateAuthorizedAsync();
        var vault = await _vaultDao.CreateAsync(owner.Id, "Owner Vault", new byte[] { 1, 2, 3 });
        owner.Delete();
        await DbSessionProvider.CurrentSession.UpdateAsync(owner);
        await FlushDbChanges();

        await Assert.ThrowsAsync<HasNoAccessException>(() =>
            _securityService.CheckAccess(AccessLevel.Read, owner, vault));
    }

    [Fact]
    public async Task ShouldNotThrowIfUserHasAccess()
    {
        var (_, owner) = await UserSeeder.CreateAuthorizedAsync();
        var vault = await _vaultDao.CreateAsync(owner.Id, "Owner Vault", new byte[] { 1, 2, 3 });
        await FlushDbChanges();

        var exception = await Record.ExceptionAsync(() =>
            _securityService.CheckAccess(AccessLevel.Read, owner, vault));
        Assert.Null(exception);

        exception = await Record.ExceptionAsync(() =>
            _securityService.CheckAccess(AccessLevel.Write, owner.Id, vault));
        Assert.Null(exception);
    }
}


