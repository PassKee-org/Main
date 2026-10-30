using System;
using System.Collections.Generic;
using PassKee.Api.Shared.Models.Storage;
using PassKee.Api.Shared.Models.Vaults.Payloads;
using PassKee.Business.Common.Constants;
using PassKee.Web.Core.Utils;
using PassKee.Web.Models.Vaults;
using Xunit;

namespace PassKee.Tests.Unit.Web.Core.Utils;

public class CredentialAttachmentUtilsTests
{
    [Fact]
    public void CollectAttachmentFileIds_NullOrEmptyPayload_ReturnsEmpty()
    {
        Assert.Empty(CredentialAttachmentUtils.CollectAttachmentFileIds(null));
        Assert.Empty(CredentialAttachmentUtils.CollectAttachmentFileIds(new LoginCredentialPayload()));
    }

    [Fact]
    public void CollectAttachmentFileIds_CollectsFromAllSources()
    {
        var fileId1 = Guid.NewGuid();
        var fileId2 = Guid.NewGuid();
        var fileId3 = Guid.NewGuid();

        var payload = new FileCredentialPayload
        {
            File = new StoredFileDto { Id = fileId1 },
            AdditionalFields =
            [
                new CredentialField
                {
                    Type = CredentialFieldType.File,
                    File = new StoredFileDto { Id = fileId2 }
                },
                new CredentialField
                {
                    Type = CredentialFieldType.Text,
                    Value = "Not a file"
                }
            ],
            Sections =
            [
                new CredentialSection
                {
                    Fields =
                    [
                        new CredentialField
                        {
                            Type = CredentialFieldType.File,
                            File = new StoredFileDto { Id = fileId3 }
                        }
                    ]
                }
            ]
        };

        var collected = CredentialAttachmentUtils.CollectAttachmentFileIds(payload);

        Assert.Equal(3, collected.Count);
        Assert.Contains(fileId1, collected);
        Assert.Contains(fileId2, collected);
        Assert.Contains(fileId3, collected);
    }

    [Fact]
    public void CollectAttachmentFileIds_SshKey_CollectsPrivateKeyFile()
    {
        var fileId = Guid.NewGuid();
        var payload = new SshKeyCredentialPayload
        {
            PrivateKeyFile = new StoredFileDto { Id = fileId }
        };

        var collected = CredentialAttachmentUtils.CollectAttachmentFileIds(payload);
        Assert.Single(collected);
        Assert.Equal(fileId, collected[0]);
    }

    [Fact]
    public void FindOrphanFileIds_WhenFileIsNotReferencedElsewhere_ReturnsOrphan()
    {
        var orphanFileId = Guid.NewGuid();
        var vaultId = Guid.NewGuid();

        var credToDelete = new DecryptedCredential(
            Guid.NewGuid(),
            vaultId,
            null,
            CredentialType.File,
            new FileCredentialPayload { File = new StoredFileDto { Id = orphanFileId } }
        );

        var remainingCred = new DecryptedCredential(
            Guid.NewGuid(),
            vaultId,
            null,
            CredentialType.Login,
            new LoginCredentialPayload()
        );

        var orphans = CredentialAttachmentUtils.FindOrphanFileIds(
            [credToDelete],
            [credToDelete, remainingCred]
        );

        Assert.Single(orphans);
        Assert.Equal(orphanFileId, orphans[0]);
    }

    [Fact]
    public void FindOrphanFileIds_WhenFileIsSharedWithRemainingEntry_ReturnsEmpty()
    {
        var sharedFileId = Guid.NewGuid();
        var vaultId = Guid.NewGuid();

        var credToDelete = new DecryptedCredential(
            Guid.NewGuid(),
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

        var orphans = CredentialAttachmentUtils.FindOrphanFileIds(
            [credToDelete],
            [credToDelete, remainingCred]
        );

        Assert.Empty(orphans);
    }

    [Fact]
    public void FindOrphanFileIds_BulkDelete_HandlesSharedBetweenDeletingAndOtherEntries()
    {
        var sharedWithOther = Guid.NewGuid();
        var orphanBetweenTwoDeleting = Guid.NewGuid();
        var vaultId = Guid.NewGuid();

        var credToDelete1 = new DecryptedCredential(
            Guid.NewGuid(),
            vaultId,
            null,
            CredentialType.File,
            new FileCredentialPayload { File = new StoredFileDto { Id = sharedWithOther } }
        );

        var credToDelete2 = new DecryptedCredential(
            Guid.NewGuid(),
            vaultId,
            null,
            CredentialType.File,
            new FileCredentialPayload { File = new StoredFileDto { Id = orphanBetweenTwoDeleting } }
        );

        var remainingCred = new DecryptedCredential(
            Guid.NewGuid(),
            vaultId,
            null,
            CredentialType.File,
            new FileCredentialPayload { File = new StoredFileDto { Id = sharedWithOther } }
        );

        var orphans = CredentialAttachmentUtils.FindOrphanFileIds(
            [credToDelete1, credToDelete2],
            [credToDelete1, credToDelete2, remainingCred]
        );

        Assert.Single(orphans);
        Assert.Equal(orphanBetweenTwoDeleting, orphans[0]);
    }
}

