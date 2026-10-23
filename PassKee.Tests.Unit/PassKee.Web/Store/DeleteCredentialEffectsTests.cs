using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fluxor;
using Moq;
using PassKee.Api.Shared.Models.Storage;
using PassKee.Api.Shared.Models.Vaults;
using PassKee.Api.Shared.Models.Vaults.Payloads;
using PassKee.Business.Common.Constants;
using PassKee.Web.Core.Services.UI.Toast;
using PassKee.Web.Models.Vaults;
using PassKee.Web.Services.Vaults;
using PassKee.Web.Store.Vaults;
using PassKee.Web.Store.Vaults.Effects;
using Xunit;

namespace PassKee.Tests.Unit.Web.Store;

public class DeleteCredentialEffectsTests
{
    private readonly Mock<IVaultClientService> _vaultServiceMock = new();
    private readonly Mock<IToastService> _toastServiceMock = new();
    private readonly Mock<IDispatcher> _dispatcherMock = new();
    private readonly Mock<IState<VaultsState>> _vaultsStateMock = new();

    [Fact]
    public async Task DeleteCredentialEffect_WhenFileIsOrphan_DeletesFile()
    {
        var vaultId = Guid.NewGuid();
        var credId = Guid.NewGuid();
        var fileId = Guid.NewGuid();

        var credToDelete = new DecryptedCredential(
            credId,
            vaultId,
            null,
            CredentialType.File,
            new FileCredentialPayload { File = new StoredFileDto { Id = fileId } }
        );

        var otherCred = new DecryptedCredential(
            Guid.NewGuid(),
            vaultId,
            null,
            CredentialType.Login,
            new LoginCredentialPayload()
        );

        var state = new VaultsState
        {
            ActiveVaultId = vaultId,
            Credentials = [credToDelete, otherCred],
            ArchivedCredentials = []
        };
        _vaultsStateMock.Setup(s => s.Value).Returns(state);

        _vaultServiceMock
            .Setup(s => s.DeleteCredentialAsync(credId))
            .ReturnsAsync(new DeleteCredentialResponse { CredentialId = credId });

        _vaultServiceMock
            .Setup(s => s.DeleteFileAsync(fileId))
            .ReturnsAsync(true);

        var effect = new DeleteCredentialEffect(_vaultServiceMock.Object, _toastServiceMock.Object, _vaultsStateMock.Object);
        await effect.HandleAsync(new DeleteCredentialAction(vaultId, credId), _dispatcherMock.Object);

        _vaultServiceMock.Verify(s => s.DeleteCredentialAsync(credId), Times.Once);
        _vaultServiceMock.Verify(s => s.DeleteFileAsync(fileId), Times.Once);
        _dispatcherMock.Verify(d => d.Dispatch(It.Is<DeleteCredentialSuccessAction>(a => a.VaultId == vaultId && a.CredentialId == credId)), Times.Once);
    }

    [Fact]
    public async Task DeleteCredentialEffect_WhenFileExistsInAnotherEntry_DoesNotDeleteFile()
    {
        var vaultId = Guid.NewGuid();
        var credId = Guid.NewGuid();
        var sharedFileId = Guid.NewGuid();

        var credToDelete = new DecryptedCredential(
            credId,
            vaultId,
            null,
            CredentialType.File,
            new FileCredentialPayload { File = new StoredFileDto { Id = sharedFileId } }
        );

        var remainingCred = new DecryptedCredential(
            Guid.NewGuid(),
            vaultId,
            null,
            CredentialType.File,
            new FileCredentialPayload { File = new StoredFileDto { Id = sharedFileId } }
        );

        var state = new VaultsState
        {
            ActiveVaultId = vaultId,
            Credentials = [credToDelete, remainingCred],
            ArchivedCredentials = []
        };
        _vaultsStateMock.Setup(s => s.Value).Returns(state);

        _vaultServiceMock
            .Setup(s => s.DeleteCredentialAsync(credId))
            .ReturnsAsync(new DeleteCredentialResponse { CredentialId = credId });

        var effect = new DeleteCredentialEffect(_vaultServiceMock.Object, _toastServiceMock.Object, _vaultsStateMock.Object);
        await effect.HandleAsync(new DeleteCredentialAction(vaultId, credId), _dispatcherMock.Object);

        _vaultServiceMock.Verify(s => s.DeleteCredentialAsync(credId), Times.Once);
        _vaultServiceMock.Verify(s => s.DeleteFileAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task DeleteCredentialsEffect_BulkDelete_OnlyDeletesTrueOrphanFiles()
    {
        var vaultId = Guid.NewGuid();
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();
        var sharedFileId = Guid.NewGuid();
        var orphanFileId = Guid.NewGuid();

        var credToDelete1 = new DecryptedCredential(
            id1,
            vaultId,
            null,
            CredentialType.File,
            new FileCredentialPayload { File = new StoredFileDto { Id = sharedFileId } }
        );

        var credToDelete2 = new DecryptedCredential(
            id2,
            vaultId,
            null,
            CredentialType.File,
            new FileCredentialPayload { File = new StoredFileDto { Id = orphanFileId } }
        );

        var remainingCred = new DecryptedCredential(
            Guid.NewGuid(),
            vaultId,
            null,
            CredentialType.File,
            new FileCredentialPayload { File = new StoredFileDto { Id = sharedFileId } }
        );

        var state = new VaultsState
        {
            ActiveVaultId = vaultId,
            Credentials = [remainingCred],
            ArchivedCredentials = [credToDelete1, credToDelete2]
        };
        _vaultsStateMock.Setup(s => s.Value).Returns(state);

        _vaultServiceMock
            .Setup(s => s.DeleteCredentialsAsync(It.IsAny<List<Guid>>()))
            .ReturnsAsync(new DeleteCredentialsResponse { CredentialIds = [id1, id2] });

        _vaultServiceMock
            .Setup(s => s.DeleteFileAsync(orphanFileId))
            .ReturnsAsync(true);

        var effect = new DeleteCredentialsEffect(_vaultServiceMock.Object, _toastServiceMock.Object, _vaultsStateMock.Object);
        await effect.HandleAsync(new DeleteCredentialsAction(vaultId, [id1, id2]), _dispatcherMock.Object);

        _vaultServiceMock.Verify(s => s.DeleteCredentialsAsync(It.Is<List<Guid>>(l => l.Contains(id1) && l.Contains(id2))), Times.Once);
        _vaultServiceMock.Verify(s => s.DeleteFileAsync(orphanFileId), Times.Once);
        _vaultServiceMock.Verify(s => s.DeleteFileAsync(sharedFileId), Times.Never);
        _dispatcherMock.Verify(d => d.Dispatch(It.Is<DeleteCredentialsSuccessAction>(a => a.VaultId == vaultId && a.CredentialIds.Count == 2)), Times.Once);
    }
}

