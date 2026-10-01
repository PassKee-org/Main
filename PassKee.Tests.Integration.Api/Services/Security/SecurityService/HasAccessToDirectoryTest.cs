using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using PassKee.Api.Services.Security;
using PassKee.Business.Common.Constants;
using PassKee.Orm.Dao.Vaults;
using PassKee.Orm.Entities.Vaults;
using PassKee.Tests.Integration.Api.Core;
using Xunit;

namespace PassKee.Tests.Integration.Api.Services.Security.SecurityService;

public class HasAccessToDirectoryTest : BaseTest
{
    private readonly ISecurityService _securityService;
    private readonly IVaultDao _vaultDao;
    private readonly IDirectoryDao _directoryDao;

    public HasAccessToDirectoryTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        _securityService = ServiceProvider.GetRequiredService<ISecurityService>();
        _vaultDao = ServiceProvider.GetRequiredService<IVaultDao>();
        _directoryDao = ServiceProvider.GetRequiredService<IDirectoryDao>();
    }

    [Theory]
    [InlineData(AccessLevel.Read)]
    [InlineData(AccessLevel.Write)]
    public async Task ShouldHaveAccessIfUserOwnsVaultOfDirectory(AccessLevel accessLevel)
    {
        var (_, owner) = await UserSeeder.CreateAuthorizedAsync();
        var vault = await _vaultDao.CreateAsync(owner.Id, "Owner Vault", new byte[] { 1, 2, 3 });
        var dir = await _directoryDao.CreateAsync(vault.Id, null, new byte[] { 10, 20 });
        await FlushDbChanges();

        var hasAccess = await _securityService.HasAccess(accessLevel, owner, dir);
        Assert.True(hasAccess);

        var hasAccessById = await _securityService.HasAccess(accessLevel, owner.Id, dir);
        Assert.True(hasAccessById);
    }

    [Theory]
    [InlineData(AccessLevel.Read)]
    [InlineData(AccessLevel.Write)]
    public async Task ShouldNotHaveAccessIfUserDoesNotOwnVault(AccessLevel accessLevel)
    {
        var (_, owner) = await UserSeeder.CreateAuthorizedAsync();
        var (_, otherUser) = await UserSeeder.CreateAuthorizedAsync();
        var vault = await _vaultDao.CreateAsync(owner.Id, "Owner Vault", new byte[] { 1, 2, 3 });
        var dir = await _directoryDao.CreateAsync(vault.Id, null, new byte[] { 10, 20 });
        await FlushDbChanges();

        var hasAccess = await _securityService.HasAccess(accessLevel, otherUser, dir);
        Assert.False(hasAccess);

        var hasAccessById = await _securityService.HasAccess(accessLevel, otherUser.Id, dir);
        Assert.False(hasAccessById);
    }

    [Theory]
    [InlineData(AccessLevel.Read)]
    [InlineData(AccessLevel.Write)]
    public async Task ShouldNotHaveAccessIfDirectoryIsSoftDeleted(AccessLevel accessLevel)
    {
        var (_, owner) = await UserSeeder.CreateAuthorizedAsync();
        var vault = await _vaultDao.CreateAsync(owner.Id, "Owner Vault", new byte[] { 1, 2, 3 });
        var dir = await _directoryDao.CreateAsync(vault.Id, null, new byte[] { 10, 20 });
        await _directoryDao.DeleteAsync(dir);
        await FlushDbChanges();

        var hasAccess = await _securityService.HasAccess(accessLevel, owner, dir);
        Assert.False(hasAccess);

        var hasAccessById = await _securityService.HasAccess(accessLevel, owner.Id, dir);
        Assert.False(hasAccessById);
    }

    [Theory]
    [InlineData(AccessLevel.Read)]
    [InlineData(AccessLevel.Write)]
    public async Task ShouldNotHaveAccessIfVaultIsSoftDeleted(AccessLevel accessLevel)
    {
        var (_, owner) = await UserSeeder.CreateAuthorizedAsync();
        var vault = await _vaultDao.CreateAsync(owner.Id, "Owner Vault", new byte[] { 1, 2, 3 });
        var dir = await _directoryDao.CreateAsync(vault.Id, null, new byte[] { 10, 20 });
        await _vaultDao.DeleteAsync(vault);
        await FlushDbChanges();

        var hasAccess = await _securityService.HasAccess(accessLevel, owner, dir);
        Assert.False(hasAccess);

        var hasAccessById = await _securityService.HasAccess(accessLevel, owner.Id, dir);
        Assert.False(hasAccessById);
    }
}


