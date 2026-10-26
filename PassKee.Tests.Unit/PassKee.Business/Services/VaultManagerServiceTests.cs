using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using PassKee.Business.Services.Storage;
using PassKee.Business.Services.Vaults;
using PassKee.Orm.Dao.Vaults;
using PassKee.Orm.Entities.Vaults;
using Xunit;

namespace PassKee.Tests.Unit.Business.Services;

public class VaultManagerServiceTests
{
    private readonly Mock<IVaultDao> _vaultDaoMock = new();
    private readonly Mock<ICredentialDao> _credentialDaoMock = new();
    private readonly Mock<IDirectoryDao> _directoryDaoMock = new();
    private readonly Mock<ITagDao> _tagDaoMock = new();
    private readonly Mock<IFileStorageService> _fileStorageServiceMock = new();

    private VaultManagerService CreateService() => new(
        _vaultDaoMock.Object,
        _credentialDaoMock.Object,
        _directoryDaoMock.Object,
        _tagDaoMock.Object,
        _fileStorageServiceMock.Object);

    [Fact]
    public async Task DeleteVaultAsync_WithEntity_DeletesRelatedDataAndVault()
    {
        var vaultId = Guid.NewGuid();
        var vault = new VaultEntity { Id = vaultId, Name = "Test Vault" };
        var service = CreateService();

        await service.DeleteVaultAsync(vault);

        _fileStorageServiceMock.Verify(s => s.DeleteVaultFilesAsync(vaultId, It.IsAny<CancellationToken>()), Times.Once);
        _credentialDaoMock.Verify(d => d.DeleteByVaultIdAsync(vaultId, It.IsAny<CancellationToken>()), Times.Once);
        _directoryDaoMock.Verify(d => d.DeleteByVaultIdAsync(vaultId, It.IsAny<CancellationToken>()), Times.Once);
        _tagDaoMock.Verify(d => d.DeleteByVaultIdAsync(vaultId, It.IsAny<CancellationToken>()), Times.Once);
        _vaultDaoMock.Verify(d => d.DeleteAsync(vault, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteVaultAsync_WithGuid_LoadsVaultAndDeletes()
    {
        var vaultId = Guid.NewGuid();
        var vault = new VaultEntity { Id = vaultId, Name = "Test Vault" };
        _vaultDaoMock.Setup(d => d.GetById(vaultId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(vault);

        var service = CreateService();
        await service.DeleteVaultAsync(vaultId);

        _fileStorageServiceMock.Verify(s => s.DeleteVaultFilesAsync(vaultId, It.IsAny<CancellationToken>()), Times.Once);
        _credentialDaoMock.Verify(d => d.DeleteByVaultIdAsync(vaultId, It.IsAny<CancellationToken>()), Times.Once);
        _directoryDaoMock.Verify(d => d.DeleteByVaultIdAsync(vaultId, It.IsAny<CancellationToken>()), Times.Once);
        _tagDaoMock.Verify(d => d.DeleteByVaultIdAsync(vaultId, It.IsAny<CancellationToken>()), Times.Once);
        _vaultDaoMock.Verify(d => d.DeleteAsync(vault, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteVaultAsync_WithGuid_WhenVaultNotFound_DoesNothing()
    {
        var vaultId = Guid.NewGuid();
        _vaultDaoMock.Setup(d => d.GetById(vaultId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((VaultEntity?)null);

        var service = CreateService();
        await service.DeleteVaultAsync(vaultId);

        _fileStorageServiceMock.Verify(s => s.DeleteVaultFilesAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _vaultDaoMock.Verify(d => d.DeleteAsync(It.IsAny<VaultEntity>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteVaultAsync_WithNullEntity_ThrowsArgumentNullException()
    {
        var service = CreateService();
        await Assert.ThrowsAsync<ArgumentNullException>(() => service.DeleteVaultAsync((VaultEntity)null!));
    }
}

