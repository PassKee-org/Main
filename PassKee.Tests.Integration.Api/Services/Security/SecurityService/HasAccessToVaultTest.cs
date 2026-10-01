using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using PassKee.Api.Services.Security;
using PassKee.Business.Common.Constants;
using PassKee.Orm.Dao.Vaults;
using PassKee.Orm.Entities;
using PassKee.Orm.Entities.Vaults;
using PassKee.Tests.Integration.Api.Core;
using Xunit;

namespace PassKee.Tests.Integration.Api.Services.Security.SecurityService;

public class HasAccessToVaultTest : BaseTest
{
    private readonly ISecurityService _securityService;
    private readonly IVaultDao _vaultDao;

    public HasAccessToVaultTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        _securityService = ServiceProvider.GetRequiredService<ISecurityService>();
        _vaultDao = ServiceProvider.GetRequiredService<IVaultDao>();
    }

    [Theory]
    [InlineData(AccessLevel.Read)]
    [InlineData(AccessLevel.Write)]
    public async Task ShouldHaveAccessIfUserIsOwner(AccessLevel accessLevel)
    {
        var (_, owner) = await UserSeeder.CreateAuthorizedAsync();
        var vault = await _vaultDao.CreateAsync(owner.Id, "Owner Vault", new byte[] { 1, 2, 3 });
        await FlushDbChanges();

        var hasAccess = await _securityService.HasAccess(accessLevel, owner, vault);
        Assert.True(hasAccess);

        var hasAccessById = await _securityService.HasAccess(accessLevel, owner.Id, vault);
        Assert.True(hasAccessById);
    }

    [Theory]
    [InlineData(AccessLevel.Read)]
    [InlineData(AccessLevel.Write)]
    public async Task ShouldNotHaveAccessIfUserIsNotOwner(AccessLevel accessLevel)
    {
        var (_, owner) = await UserSeeder.CreateAuthorizedAsync();
        var (_, otherUser) = await UserSeeder.CreateAuthorizedAsync();
        var vault = await _vaultDao.CreateAsync(owner.Id, "Owner Vault", new byte[] { 1, 2, 3 });
        await FlushDbChanges();

        var hasAccess = await _securityService.HasAccess(accessLevel, otherUser, vault);
        Assert.False(hasAccess);

        var hasAccessById = await _securityService.HasAccess(accessLevel, otherUser.Id, vault);
        Assert.False(hasAccessById);
    }

    [Theory]
    [InlineData(AccessLevel.Read)]
    [InlineData(AccessLevel.Write)]
    public async Task ShouldNotHaveAccessIfVaultIsSoftDeleted(AccessLevel accessLevel)
    {
        var (_, owner) = await UserSeeder.CreateAuthorizedAsync();
        var vault = await _vaultDao.CreateAsync(owner.Id, "Owner Vault", new byte[] { 1, 2, 3 });
        await _vaultDao.DeleteAsync(vault);
        await FlushDbChanges();

        var hasAccess = await _securityService.HasAccess(accessLevel, owner, vault);
        Assert.False(hasAccess);

        var hasAccessById = await _securityService.HasAccess(accessLevel, owner.Id, vault);
        Assert.False(hasAccessById);
    }

    [Theory]
    [InlineData(AccessLevel.Read)]
    [InlineData(AccessLevel.Write)]
    public async Task ShouldNotHaveAccessIfUserIsSoftDeleted(AccessLevel accessLevel)
    {
        var (_, owner) = await UserSeeder.CreateAuthorizedAsync();
        var vault = await _vaultDao.CreateAsync(owner.Id, "Owner Vault", new byte[] { 1, 2, 3 });
        owner.Delete();
        await DbSessionProvider.CurrentSession.UpdateAsync(owner);
        await FlushDbChanges();

        var hasAccess = await _securityService.HasAccess(accessLevel, owner, vault);
        Assert.False(hasAccess);
    }
}

