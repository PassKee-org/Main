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
    public void ReduceDeleteDirectorySuccessAction_RemovesDirectoryDescendantsAndNestedCredentials()
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
}
