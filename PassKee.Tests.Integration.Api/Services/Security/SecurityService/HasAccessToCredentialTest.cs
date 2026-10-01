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

public class HasAccessToCredentialTest : BaseTest
{
    private readonly ISecurityService _securityService;
    private readonly IVaultDao _vaultDao;
    private readonly ICredentialDao _credentialDao;

    public HasAccessToCredentialTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        _securityService = ServiceProvider.GetRequiredService<ISecurityService>();
        _vaultDao = ServiceProvider.GetRequiredService<IVaultDao>();
        _credentialDao = ServiceProvider.GetRequiredService<ICredentialDao>();
    }

    [Theory]
    [InlineData(AccessLevel.Read)]
    [InlineData(AccessLevel.Write)]
    public async Task ShouldHaveAccessIfUserOwnsVaultOfCredential(AccessLevel accessLevel)
    {
        var (_, owner) = await UserSeeder.CreateAuthorizedAsync();
        var vault = await _vaultDao.CreateAsync(owner.Id, "Owner Vault", new byte[] { 1, 2, 3 });
        var cred = await _credentialDao.CreateAsync(vault.Id, null, CredentialType.Login, new byte[] { 10, 20 });
        await FlushDbChanges();

        var hasAccess = await _securityService.HasAccess(accessLevel, owner, cred);
        Assert.True(hasAccess);

        var hasAccessById = await _securityService.HasAccess(accessLevel, owner.Id, cred);
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
        var cred = await _credentialDao.CreateAsync(vault.Id, null, CredentialType.Login, new byte[] { 10, 20 });
        await FlushDbChanges();

        var hasAccess = await _securityService.HasAccess(accessLevel, otherUser, cred);
        Assert.False(hasAccess);

        var hasAccessById = await _securityService.HasAccess(accessLevel, otherUser.Id, cred);
        Assert.False(hasAccessById);
    }

    [Theory]
    [InlineData(AccessLevel.Read)]
    [InlineData(AccessLevel.Write)]
    public async Task ShouldNotHaveAccessIfCredentialIsSoftDeleted(AccessLevel accessLevel)
    {
        var (_, owner) = await UserSeeder.CreateAuthorizedAsync();
        var vault = await _vaultDao.CreateAsync(owner.Id, "Owner Vault", new byte[] { 1, 2, 3 });
        var cred = await _credentialDao.CreateAsync(vault.Id, null, CredentialType.Login, new byte[] { 10, 20 });
        await _credentialDao.DeleteAsync(cred);
        await FlushDbChanges();

        var hasAccess = await _securityService.HasAccess(accessLevel, owner, cred);
        Assert.False(hasAccess);

        var hasAccessById = await _securityService.HasAccess(accessLevel, owner.Id, cred);
        Assert.False(hasAccessById);
    }

    [Theory]
    [InlineData(AccessLevel.Read)]
    [InlineData(AccessLevel.Write)]
    public async Task ShouldNotHaveAccessIfVaultIsSoftDeleted(AccessLevel accessLevel)
    {
        var (_, owner) = await UserSeeder.CreateAuthorizedAsync();
        var vault = await _vaultDao.CreateAsync(owner.Id, "Owner Vault", new byte[] { 1, 2, 3 });
        var cred = await _credentialDao.CreateAsync(vault.Id, null, CredentialType.Login, new byte[] { 10, 20 });
        await _vaultDao.DeleteAsync(vault);
        await FlushDbChanges();

        var hasAccess = await _securityService.HasAccess(accessLevel, owner, cred);
        Assert.False(hasAccess);

        var hasAccessById = await _securityService.HasAccess(accessLevel, owner.Id, cred);
        Assert.False(hasAccessById);
    }
}


