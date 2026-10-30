using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fluxor;
using Moq;
using PassKee.Api.Shared.Models.Vaults;
using PassKee.Web.Core.Services.UI.Toast;
using PassKee.Web.Services.Vaults;
using PassKee.Web.Store.Vaults;
using PassKee.Web.Store.Vaults.Effects;
using Xunit;

namespace PassKee.Tests.Unit.Web.Store;

public class DeleteVaultEffectTests
{
    private readonly Mock<IVaultClientService> _vaultServiceMock = new();
    private readonly Mock<IToastService> _toastServiceMock = new();
    private readonly Mock<IDispatcher> _dispatcherMock = new();
    private readonly Mock<IState<VaultsState>> _vaultsStateMock = new();

    [Fact]
    public async Task HandleAsync_WhenSuccessful_DispatchesSuccessAndSelectsNextVault()
    {
        var vaultId1 = Guid.NewGuid();
        var vaultId2 = Guid.NewGuid();
        var vault1 = new VaultDto { Id = vaultId1, Name = "Vault 1" };
        var vault2 = new VaultDto { Id = vaultId2, Name = "Vault 2" };

        var state = new VaultsState
        {
            Vaults = [vault1, vault2],
            ActiveVaultId = vaultId1
        };
        _vaultsStateMock.Setup(s => s.Value).Returns(state);

        _vaultServiceMock
            .Setup(s => s.DeleteVaultAsync(vaultId1))
            .ReturnsAsync(new DeleteVaultResponse { VaultId = vaultId1 });

        var effect = new DeleteVaultEffect(_vaultServiceMock.Object, _toastServiceMock.Object, _vaultsStateMock.Object);
        await effect.HandleAsync(new DeleteVaultAction(vaultId1), _dispatcherMock.Object);

        _vaultServiceMock.Verify(s => s.DeleteVaultAsync(vaultId1), Times.Once);
        _dispatcherMock.Verify(d => d.Dispatch(It.Is<DeleteVaultSuccessAction>(a => a.VaultId == vaultId1)), Times.Once);
        _dispatcherMock.Verify(d => d.Dispatch(It.Is<SelectVaultAction>(a => a.VaultId == vaultId2)), Times.Once);
        _dispatcherMock.Verify(d => d.Dispatch(It.Is<LoadVaultDetailsAction>(a => a.VaultId == vaultId2)), Times.Once);
        _toastServiceMock.Verify(t => t.ShowSuccess("Vault deleted"), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenLastVaultDeleted_DispatchesSuccessWithoutSelectingNext()
    {
        var vaultId = Guid.NewGuid();
        var vault = new VaultDto { Id = vaultId, Name = "Solo Vault" };

        var state = new VaultsState
        {
            Vaults = [vault],
            ActiveVaultId = vaultId
        };
        _vaultsStateMock.Setup(s => s.Value).Returns(state);

        _vaultServiceMock
            .Setup(s => s.DeleteVaultAsync(vaultId))
            .ReturnsAsync(new DeleteVaultResponse { VaultId = vaultId });

        var effect = new DeleteVaultEffect(_vaultServiceMock.Object, _toastServiceMock.Object, _vaultsStateMock.Object);
        await effect.HandleAsync(new DeleteVaultAction(vaultId), _dispatcherMock.Object);

        _dispatcherMock.Verify(d => d.Dispatch(It.Is<DeleteVaultSuccessAction>(a => a.VaultId == vaultId)), Times.Once);
        _dispatcherMock.Verify(d => d.Dispatch(It.IsAny<SelectVaultAction>()), Times.Never);
        _dispatcherMock.Verify(d => d.Dispatch(It.IsAny<LoadVaultDetailsAction>()), Times.Never);
        _toastServiceMock.Verify(t => t.ShowSuccess("Vault deleted"), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenServiceReturnsNull_DispatchesFailure()
    {
        var vaultId = Guid.NewGuid();
        var state = new VaultsState { ActiveVaultId = vaultId };
        _vaultsStateMock.Setup(s => s.Value).Returns(state);

        _vaultServiceMock
            .Setup(s => s.DeleteVaultAsync(vaultId))
            .ReturnsAsync((DeleteVaultResponse?)null);

        var effect = new DeleteVaultEffect(_vaultServiceMock.Object, _toastServiceMock.Object, _vaultsStateMock.Object);
        await effect.HandleAsync(new DeleteVaultAction(vaultId), _dispatcherMock.Object);

        _dispatcherMock.Verify(d => d.Dispatch(It.IsAny<DeleteVaultFailureAction>()), Times.Once);
        _dispatcherMock.Verify(d => d.Dispatch(It.IsAny<DeleteVaultSuccessAction>()), Times.Never);
        _toastServiceMock.Verify(t => t.ShowError("Failed to delete vault"), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenExceptionThrown_DispatchesFailure()
    {
        var vaultId = Guid.NewGuid();
        var state = new VaultsState { ActiveVaultId = vaultId };
        _vaultsStateMock.Setup(s => s.Value).Returns(state);

        _vaultServiceMock
            .Setup(s => s.DeleteVaultAsync(vaultId))
            .ThrowsAsync(new Exception("Network error"));

        var effect = new DeleteVaultEffect(_vaultServiceMock.Object, _toastServiceMock.Object, _vaultsStateMock.Object);
        await effect.HandleAsync(new DeleteVaultAction(vaultId), _dispatcherMock.Object);

        _dispatcherMock.Verify(d => d.Dispatch(It.IsAny<DeleteVaultFailureAction>()), Times.Once);
        _dispatcherMock.Verify(d => d.Dispatch(It.IsAny<DeleteVaultSuccessAction>()), Times.Never);
        _toastServiceMock.Verify(t => t.ShowError("Error deleting vault"), Times.Once);
    }
}

