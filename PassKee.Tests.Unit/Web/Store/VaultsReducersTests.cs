using System;
using System.Collections.Generic;
using PassKee.Api.Shared.Models.Vaults.Payloads;
using PassKee.Business.Common.Constants;
using PassKee.Web.Models.Vaults;
using PassKee.Web.Store.Vaults;
using Xunit;

namespace PassKee.Tests.Unit.Web.Store;

public class VaultsReducersTests
{
    [Fact]
    public void ReduceCreateDirectorySuccessAction_AppendsDirectory()
    {
        var state = new VaultsState();
        var dir = new DecryptedDirectory(Guid.NewGuid(), Guid.NewGuid(), null, "Folder 1");

        var nextState = VaultsReducers.ReduceCreateDirectorySuccessAction(state, new CreateDirectorySuccessAction(dir));

        Assert.Single(nextState.Directories);
        Assert.Equal(dir, nextState.Directories[0]);
    }

    [Fact]
    public void ReduceUpdateDirectorySuccessAction_UpdatesExistingDirectory()
    {
        var dirId = Guid.NewGuid();
        var vaultId = Guid.NewGuid();
        var originalDir = new DecryptedDirectory(dirId, vaultId, null, "Old Name");
        var state = new VaultsState
        {
            Directories = [originalDir]
        };

        var updatedDir = new DecryptedDirectory(dirId, vaultId, null, "New Name");
        var nextState = VaultsReducers.ReduceUpdateDirectorySuccessAction(state, new UpdateDirectorySuccessAction(updatedDir));

        Assert.Single(nextState.Directories);
        Assert.Equal("New Name", nextState.Directories[0].Name);
    }

    [Fact]
    public void ReduceDeleteDirectorySuccessAction_RemovesDirectoryDescendantsAndArchivesNestedCredentials()
    {
        var vaultId = Guid.NewGuid();
        var parentId = Guid.NewGuid();
        var childId = Guid.NewGuid();
        var otherId = Guid.NewGuid();

        var parentDir = new DecryptedDirectory(parentId, vaultId, null, "Parent");
        var childDir = new DecryptedDirectory(childId, vaultId, parentId, "Child");
        var otherDir = new DecryptedDirectory(otherId, vaultId, null, "Other");

        var credInParent = new DecryptedCredential(Guid.NewGuid(), vaultId, parentId, CredentialType.Login, new LoginCredentialPayload { Title = "In Parent" });
        var credInChild = new DecryptedCredential(Guid.NewGuid(), vaultId, childId, CredentialType.Login, new LoginCredentialPayload { Title = "In Child" });
        var credInOther = new DecryptedCredential(Guid.NewGuid(), vaultId, otherId, CredentialType.Login, new LoginCredentialPayload { Title = "In Other" });

        var state = new VaultsState
        {
            Directories = [parentDir, childDir, otherDir],
            Credentials = [credInParent, credInChild, credInOther],
            ArchivedCredentials = [],
            SelectedDirectoryId = childId
        };

        var nextState = VaultsReducers.ReduceDeleteDirectorySuccessAction(
            state, 
            new DeleteDirectorySuccessAction(vaultId, parentId, [parentId, childId], [credInParent.Id, credInChild.Id])
        );

        Assert.Single(nextState.Directories);
        Assert.Equal(otherId, nextState.Directories[0].Id);

        Assert.Single(nextState.Credentials);
        Assert.Equal(credInOther.Id, nextState.Credentials[0].Id);

        Assert.Equal(2, nextState.ArchivedCredentials.Count);
        Assert.Contains(nextState.ArchivedCredentials, c => c.Id == credInParent.Id && c.ArchivedAt != null);
        Assert.Contains(nextState.ArchivedCredentials, c => c.Id == credInChild.Id && c.ArchivedAt != null);

        Assert.Null(nextState.SelectedDirectoryId);
    }

    [Fact]
    public void ReduceCreateCredentialSuccessAction_AppendsCredential()
    {
        var state = new VaultsState();
        var cred = new DecryptedCredential(Guid.NewGuid(), Guid.NewGuid(), null, CredentialType.Login, new LoginCredentialPayload { Title = "New Item" });

        var nextState = VaultsReducers.ReduceCreateCredentialSuccessAction(state, new CreateCredentialSuccessAction(cred));

        Assert.Single(nextState.Credentials);
        Assert.Equal(cred, nextState.Credentials[0]);
    }

    [Fact]
    public void ReduceUpdateCredentialSuccessAction_UpdatesExistingCredential()
    {
        var credId = Guid.NewGuid();
        var vaultId = Guid.NewGuid();
        var originalCred = new DecryptedCredential(credId, vaultId, null, CredentialType.Login, new LoginCredentialPayload { Title = "Original" });
        var state = new VaultsState
        {
            Credentials = [originalCred]
        };

        var updatedCred = new DecryptedCredential(credId, vaultId, null, CredentialType.Login, new LoginCredentialPayload { Title = "Updated" });
        var nextState = VaultsReducers.ReduceUpdateCredentialSuccessAction(state, new UpdateCredentialSuccessAction(updatedCred));

        Assert.Single(nextState.Credentials);
        Assert.Equal("Updated", nextState.Credentials[0].Payload.Title);
    }

    [Fact]
    public void ReduceDeleteCredentialSuccessAction_RemovesCredential()
    {
        var credId = Guid.NewGuid();
        var vaultId = Guid.NewGuid();
        var cred = new DecryptedCredential(credId, vaultId, null, CredentialType.Login, new LoginCredentialPayload { Title = "To Delete" });
        var keepCred = new DecryptedCredential(Guid.NewGuid(), vaultId, null, CredentialType.Login, new LoginCredentialPayload { Title = "To Keep" });

        var state = new VaultsState
        {
            Credentials = [cred, keepCred]
        };

        var nextState = VaultsReducers.ReduceDeleteCredentialSuccessAction(state, new DeleteCredentialSuccessAction(vaultId, credId));

        Assert.Single(nextState.Credentials);
        Assert.Equal(keepCred.Id, nextState.Credentials[0].Id);
    }

    [Fact]
    public void ReduceSelectArchiveAction_SetsIsArchiveSelected_ClearsSelectedDirectoryIdAndSearchQuery()
    {
        var state = new VaultsState
        {
            SelectedDirectoryId = Guid.NewGuid(),
            SearchQuery = "test search",
            IsArchiveSelected = false
        };

        var nextState = VaultsReducers.ReduceSelectArchiveAction(state, new SelectArchiveAction());

        Assert.True(nextState.IsArchiveSelected);
        Assert.Null(nextState.SelectedDirectoryId);
        Assert.Equal(string.Empty, nextState.SearchQuery);
    }

    [Fact]
    public void ReduceSelectDirectoryAction_ResetsIsArchiveSelected()
    {
        var state = new VaultsState
        {
            IsArchiveSelected = true
        };
        var dirId = Guid.NewGuid();

        var nextState = VaultsReducers.ReduceSelectDirectoryAction(state, new SelectDirectoryAction(dirId));

        Assert.False(nextState.IsArchiveSelected);
        Assert.Equal(dirId, nextState.SelectedDirectoryId);
    }

    [Fact]
    public void ReduceLoadArchivedCredentialsSuccessAction_SetsArchivedCredentials()
    {
        var vaultId = Guid.NewGuid();
        var state = new VaultsState
        {
            ActiveVaultId = vaultId,
            IsArchiveLoading = true,
            ArchivedCredentials = []
        };
        var archivedCred = new DecryptedCredential(Guid.NewGuid(), vaultId, null, CredentialType.Login, new LoginCredentialPayload { Title = "Archived" }, DateTime.UtcNow);

        var nextState = VaultsReducers.ReduceLoadArchivedCredentialsSuccessAction(state, new LoadArchivedCredentialsSuccessAction(vaultId, [archivedCred]));

        Assert.False(nextState.IsArchiveLoading);
        Assert.Single(nextState.ArchivedCredentials);
        Assert.Equal(archivedCred.Id, nextState.ArchivedCredentials[0].Id);
    }

    [Fact]
    public void ReduceArchiveCredentialSuccessAction_MovesCredentialToArchivedCredentials()
    {
        var vaultId = Guid.NewGuid();
        var credId = Guid.NewGuid();
        var cred = new DecryptedCredential(credId, vaultId, null, CredentialType.Login, new LoginCredentialPayload { Title = "To Archive" });
        var otherCred = new DecryptedCredential(Guid.NewGuid(), vaultId, null, CredentialType.Login, new LoginCredentialPayload { Title = "Active" });

        var state = new VaultsState
        {
            Credentials = [cred, otherCred],
            ArchivedCredentials = []
        };

        var nextState = VaultsReducers.ReduceArchiveCredentialSuccessAction(state, new ArchiveCredentialSuccessAction(vaultId, credId));

        Assert.Single(nextState.Credentials);
        Assert.Equal(otherCred.Id, nextState.Credentials[0].Id);

        Assert.Single(nextState.ArchivedCredentials);
        Assert.Equal(credId, nextState.ArchivedCredentials[0].Id);
        Assert.NotNull(nextState.ArchivedCredentials[0].ArchivedAt);
    }

    [Fact]
    public void ReduceDeleteCredentialSuccessAction_RemovesFromArchivedCredentials()
    {
        var vaultId = Guid.NewGuid();
        var credId = Guid.NewGuid();
        var archivedCred = new DecryptedCredential(credId, vaultId, null, CredentialType.Login, new LoginCredentialPayload { Title = "Archived" }, DateTime.UtcNow);

        var state = new VaultsState
        {
            Credentials = [],
            ArchivedCredentials = [archivedCred]
        };

        var nextState = VaultsReducers.ReduceDeleteCredentialSuccessAction(state, new DeleteCredentialSuccessAction(vaultId, credId));

        Assert.Empty(nextState.ArchivedCredentials);
    }

    [Fact]
    public void ReduceDeleteCredentialsSuccessAction_RemovesMultipleCredentials()
    {
        var vaultId = Guid.NewGuid();
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();
        var id3 = Guid.NewGuid();

        var cred1 = new DecryptedCredential(id1, vaultId, null, CredentialType.Login, new LoginCredentialPayload { Title = "Active 1" });
        var cred2 = new DecryptedCredential(id2, vaultId, null, CredentialType.Login, new LoginCredentialPayload { Title = "Active 2" });
        var cred3 = new DecryptedCredential(id3, vaultId, null, CredentialType.Login, new LoginCredentialPayload { Title = "Archived 1" }, DateTime.UtcNow);

        var state = new VaultsState
        {
            Credentials = [cred1, cred2],
            ArchivedCredentials = [cred3]
        };

        var nextState = VaultsReducers.ReduceDeleteCredentialsSuccessAction(state, new DeleteCredentialsSuccessAction(vaultId, [id1, id3]));

        Assert.Single(nextState.Credentials);
        Assert.Equal(id2, nextState.Credentials[0].Id);
        Assert.Empty(nextState.ArchivedCredentials);
    }

    [Fact]
    public void ReduceUpdateCredentialPositionsAction_UpdatesOrderAndSortsCredentials()
    {
        var vaultId = Guid.NewGuid();
        var cred1 = new DecryptedCredential(Guid.NewGuid(), vaultId, null, CredentialType.Login, new LoginCredentialPayload { Title = "First" }, 0);
        var cred2 = new DecryptedCredential(Guid.NewGuid(), vaultId, null, CredentialType.Login, new LoginCredentialPayload { Title = "Second" }, 1);
        var cred3 = new DecryptedCredential(Guid.NewGuid(), vaultId, null, CredentialType.Login, new LoginCredentialPayload { Title = "Third" }, 2);

        var state = new VaultsState
        {
            ActiveVaultId = vaultId,
            Credentials = [cred1, cred2, cred3]
        };

        var positions = new Dictionary<Guid, int>
        {
            { cred3.Id, 0 },
            { cred1.Id, 1 },
            { cred2.Id, 2 }
        };

        var nextState = VaultsReducers.ReduceUpdateCredentialPositionsAction(state, new UpdateCredentialPositionsAction(vaultId, positions));

        Assert.Equal([cred3.Id, cred1.Id, cred2.Id], nextState.Credentials.Select(c => c.Id).ToArray());
        Assert.Equal([0, 1, 2], nextState.Credentials.Select(c => c.Order).ToArray());
    }

    [Fact]
    public void ReduceDeleteVaultSuccessAction_RemovesVaultAndClearsActiveVaultState()
    {
        var vaultId1 = Guid.NewGuid();
        var vaultId2 = Guid.NewGuid();
        var vault1 = new PassKee.Api.Shared.Models.Vaults.VaultDto { Id = vaultId1, Name = "Vault 1" };
        var vault2 = new PassKee.Api.Shared.Models.Vaults.VaultDto { Id = vaultId2, Name = "Vault 2" };

        var state = new VaultsState
        {
            Vaults = [vault1, vault2],
            ActiveVaultId = vaultId1,
            ActiveVaultKey = new byte[] { 1, 2, 3 },
            Directories = [new DecryptedDirectory(Guid.NewGuid(), vaultId1, null, "Dir")],
            Credentials = [new DecryptedCredential(Guid.NewGuid(), vaultId1, null, CredentialType.Login, new LoginCredentialPayload())],
            Tags = [new DecryptedTag(Guid.NewGuid(), vaultId1, "Tag")],
            SelectedDirectoryId = Guid.NewGuid(),
            IsArchiveSelected = true,
            ArchivedCredentials = [new DecryptedCredential(Guid.NewGuid(), vaultId1, null, CredentialType.Login, new LoginCredentialPayload())],
            SearchQuery = "test"
        };

        var nextState = VaultsReducers.ReduceDeleteVaultSuccessAction(state, new DeleteVaultSuccessAction(vaultId1));

        Assert.Single(nextState.Vaults);
        Assert.Equal(vaultId2, nextState.Vaults[0].Id);
        Assert.Null(nextState.ActiveVaultId);
        Assert.Null(nextState.ActiveVaultKey);
        Assert.Empty(nextState.Directories);
        Assert.Empty(nextState.Credentials);
        Assert.Empty(nextState.Tags);
        Assert.Null(nextState.SelectedDirectoryId);
        Assert.False(nextState.IsArchiveSelected);
        Assert.Empty(nextState.ArchivedCredentials);
        Assert.Empty(nextState.SearchQuery);
    }
}
