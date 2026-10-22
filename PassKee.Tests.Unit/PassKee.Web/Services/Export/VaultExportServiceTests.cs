using DgNet.Keepass;
using Moq;
using PassKee.Api.Shared.Models.Storage;
using PassKee.Api.Shared.Models.Vaults.Payloads;
using PassKee.Business.Common.Constants;
using PassKee.Web.Models.Vaults;
using PassKee.Web.Services.Export;
using PassKee.Web.Services.Import;
using PassKee.Web.Services.Vaults;

namespace PassKee.Tests.Unit.Web.Services.Export;

public class VaultExportServiceTests
{
    [Fact]
    public async Task ExportsDirectoriesActiveEntriesAndPlacesArchivedEntriesInDedicatedArchiveDirectory()
    {
        var vaultId = Guid.NewGuid();
        var dirId = Guid.NewGuid();
        var subDirId = Guid.NewGuid();
        var tagId = Guid.NewGuid();

        var directories = new List<DecryptedDirectory>
        {
            new(dirId, vaultId, null, "Work"),
            new(subDirId, vaultId, dirId, "Projects")
        };

        var activeRootCred = new DecryptedCredential(
            Guid.NewGuid(),
            vaultId,
            null,
            CredentialType.Login,
            new LoginCredentialPayload
            {
                Title = "Root Item",
                Username = "root_user",
                Password = "root_password",
                Website = "https://root.local",
                TagIds = [tagId]
            });

        var activeNestedCred = new DecryptedCredential(
            Guid.NewGuid(),
            vaultId,
            subDirId,
            CredentialType.Login,
            new LoginCredentialPayload
            {
                Title = "Project Item",
                Username = "proj_user",
                Password = "proj_password"
            });

        var archivedCredFromApi = new DecryptedCredential(
            Guid.NewGuid(),
            vaultId,
            dirId,
            CredentialType.Login,
            new LoginCredentialPayload
            {
                Title = "Archived Login",
                Username = "arch_user",
                Password = "arch_password",
                Notes = "Was in Work directory"
            },
            ArchivedAt: DateTime.UtcNow);

        var client = new Mock<IVaultClientService>();
        client.Setup(s => s.GetArchivedCredentialsAsync(vaultId, It.IsAny<byte[]>()))
            .ReturnsAsync([archivedCredFromApi]);

        var writer = new KeePassExportWriter(new Settings { Kdf = new AesKdf(new byte[32], 100) });
        var service = new VaultExportService(client.Object, writer);
        var progress = new List<ExportProgress>();

        var request = new VaultExportRequest(
            vaultId,
            "Personal",
            "export-secret",
            new byte[32],
            directories,
            [activeRootCred, activeNestedCred],
            [new DecryptedTag(tagId, vaultId, "Important")]);

        var result = await service.ExportAsync(request, progress.Add);

        Assert.False(result.Cancelled);
        Assert.Empty(result.Issues);
        Assert.Equal(2, result.Directories);
        Assert.Equal(3, result.Entries);
        Assert.Equal(1, result.ArchivedEntries);
        Assert.Equal(0, result.PartialEntries);
        Assert.NotEmpty(result.FileBytes);
        Assert.StartsWith("Personal-backup-", result.FileName);
        Assert.EndsWith(".kdbx", result.FileName);
        Assert.Equal(100, progress[^1].Percent);
        Assert.True(progress.Zip(progress.Skip(1)).All(pair => pair.First.Percent <= pair.Second.Percent));

        await using var stream = new MemoryStream(result.FileBytes);
        var imported = await new KeePassImportReader().ReadAsync(stream, "export-secret");

        Assert.Equal("Personal", imported.Name);
        var rootItem = Assert.Single(imported.Entries);
        Assert.Equal("Root Item", rootItem.Payload.Title);
        Assert.Equal("root_password", rootItem.Payload.Password);
        Assert.Equal(new[] { "Important" }, rootItem.Tags);

        var workGroup = Assert.Single(imported.Groups, g => g.Name == "Work");
        var projectsGroup = Assert.Single(workGroup.Groups, g => g.Name == "Projects");
        var projectItem = Assert.Single(projectsGroup.Entries);
        Assert.Equal("Project Item", projectItem.Payload.Title);
        Assert.Equal("proj_password", projectItem.Payload.Password);

        var archiveGroup = Assert.Single(imported.Groups, g => g.Name == VaultExportService.ArchiveDirectoryName);
        var archivedItem = Assert.Single(archiveGroup.Entries);
        Assert.Equal("Archived Login", archivedItem.Payload.Title);
        Assert.Equal("arch_user", archivedItem.Payload.Username);
        Assert.Equal("arch_password", archivedItem.Payload.Password);
        Assert.Equal("Was in Work directory", archivedItem.Payload.Notes);
    }

    [Fact]
    public async Task ExportsCardServerFileAndCustomSectionsWithAttachments()
    {
        var vaultId = Guid.NewGuid();
        var fileId = Guid.NewGuid();
        var sshFileId = Guid.NewGuid();

        var cardCred = new DecryptedCredential(
            Guid.NewGuid(),
            vaultId,
            null,
            CredentialType.Card,
            new CardCredentialPayload
            {
                Title = "Corporate Card",
                CardNumber = "4111222233334444",
                CardholderName = "Jane Doe",
                ExpirationDate = "12/29",
                Cvv = "999",
                Pin = "1234",
                Sections =
                [
                    new CredentialSection
                    {
                        Title = "Billing",
                        Fields =
                        [
                            new CredentialField { Label = "ZIP", Value = "10001", Type = CredentialFieldType.Text }
                        ]
                    }
                ]
            });

        var sshCred = new DecryptedCredential(
            Guid.NewGuid(),
            vaultId,
            null,
            CredentialType.SshKey,
            new SshKeyCredentialPayload
            {
                Title = "Prod SSH",
                PrivateKeyFile = new StoredFileDto { Id = sshFileId, FileName = "id_ed25519" },
                AdditionalFields =
                [
                    new CredentialField
                    {
                        Label = "Certificate",
                        Type = CredentialFieldType.File,
                        File = new StoredFileDto { Id = fileId, FileName = "cert.pem" }
                    }
                ]
            });

        var client = new Mock<IVaultClientService>();
        client.Setup(s => s.GetArchivedCredentialsAsync(vaultId, It.IsAny<byte[]>())).ReturnsAsync([]);
        client.Setup(s => s.DownloadFileAsync(sshFileId, It.IsAny<byte[]>())).ReturnsAsync([10, 20, 30]);
        client.Setup(s => s.DownloadFileAsync(fileId, It.IsAny<byte[]>())).ReturnsAsync([40, 50]);

        ExportGroup? capturedRoot = null;
        List<(string Name, byte[] Data)> capturedAttachments = [];
        var writer = new Mock<IKdbxExportWriter>();
        writer.Setup(w => w.WriteAsync(It.IsAny<ExportGroup>(), "pw", It.IsAny<CancellationToken>()))
            .Callback<ExportGroup, string, CancellationToken>((group, _, _) =>
            {
                capturedRoot = group;
                capturedAttachments = group.AllEntries
                    .SelectMany(e => e.Attachments)
                    .Select(a => (a.Name, a.Data.ToArray()))
                    .ToList();
            })
            .ReturnsAsync([1, 2, 3]);

        var service = new VaultExportService(client.Object, writer.Object);
        var result = await service.ExportAsync(new VaultExportRequest(
            vaultId,
            "Vault",
            "pw",
            new byte[32],
            [],
            [cardCred, sshCred],
            []));

        Assert.Equal(2, result.Entries);
        Assert.Equal(2, result.Files);
        Assert.NotNull(capturedRoot);
        Assert.DoesNotContain(capturedRoot!.Groups, g => g.Name == VaultExportService.ArchiveDirectoryName);

        var exportedCard = capturedRoot.Entries.Single(e => e.Title == "Corporate Card");
        Assert.Contains(exportedCard.CustomFields, f => f.Label == "Card Number" && f.Value == "4111222233334444" && f.IsProtected);
        Assert.Contains(exportedCard.CustomFields, f => f.Label == "CVV" && f.Value == "999" && f.IsProtected);
        Assert.Contains(exportedCard.CustomFields, f => f.Label == "Billing: ZIP" && f.Value == "10001" && !f.IsProtected);

        var exportedSsh = capturedRoot.Entries.Single(e => e.Title == "Prod SSH");
        Assert.Equal(2, exportedSsh.Attachments.Count);
        Assert.Contains(capturedAttachments, a => a.Name == "id_ed25519" && a.Data.SequenceEqual(new byte[] { 10, 20, 30 }));
        Assert.Contains(capturedAttachments, a => a.Name == "cert.pem" && a.Data.SequenceEqual(new byte[] { 40, 50 }));
        Assert.All(exportedSsh.Attachments, a => Assert.All(a.Data, b => Assert.Equal(0, b)));
    }

    [Fact]
    public async Task FailedAttachmentDownloadProducesPartialEntryAndContinuesBackup()
    {
        var vaultId = Guid.NewGuid();
        var missingFileId = Guid.NewGuid();

        var credWithFailedFile = new DecryptedCredential(
            Guid.NewGuid(),
            vaultId,
            null,
            CredentialType.File,
            new FileCredentialPayload
            {
                Title = "Broken Attachment",
                File = new StoredFileDto { Id = missingFileId, FileName = "missing.pdf" }
            });

        var healthyCred = new DecryptedCredential(
            Guid.NewGuid(),
            vaultId,
            null,
            CredentialType.Login,
            new LoginCredentialPayload { Title = "Healthy" });

        var client = new Mock<IVaultClientService>();
        client.Setup(s => s.GetArchivedCredentialsAsync(vaultId, It.IsAny<byte[]>())).ReturnsAsync([]);
        client.Setup(s => s.DownloadFileAsync(missingFileId, It.IsAny<byte[]>())).ReturnsAsync((byte[]?)null);

        var writer = new Mock<IKdbxExportWriter>();
        writer.Setup(w => w.WriteAsync(It.IsAny<ExportGroup>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([9, 9, 9]);

        var service = new VaultExportService(client.Object, writer.Object);
        var result = await service.ExportAsync(new VaultExportRequest(
            vaultId,
            "Vault",
            "pw",
            new byte[32],
            [],
            [credWithFailedFile, healthyCred],
            []));

        Assert.Equal(1, result.Entries);
        Assert.Equal(1, result.PartialEntries);
        Assert.Single(result.Issues);
        Assert.NotEmpty(result.FileBytes);
    }

    [Fact]
    public async Task LostAccessStopsExportAndClearsOutput()
    {
        var vaultId = Guid.NewGuid();
        var fileId = Guid.NewGuid();
        var cred = new DecryptedCredential(
            Guid.NewGuid(),
            vaultId,
            null,
            CredentialType.File,
            new FileCredentialPayload
            {
                Title = "Secret Doc",
                File = new StoredFileDto { Id = fileId, FileName = "secret.pdf" }
            });

        var client = new Mock<IVaultClientService>();
        client.Setup(s => s.GetArchivedCredentialsAsync(vaultId, It.IsAny<byte[]>())).ReturnsAsync([]);
        client.Setup(s => s.DownloadFileAsync(fileId, It.IsAny<byte[]>()))
            .ThrowsAsync(new HttpRequestException("unauthorized", null, System.Net.HttpStatusCode.Unauthorized));

        var writer = new Mock<IKdbxExportWriter>(MockBehavior.Strict);
        var service = new VaultExportService(client.Object, writer.Object);

        var result = await service.ExportAsync(new VaultExportRequest(
            vaultId,
            "Vault",
            "pw",
            new byte[32],
            [],
            [cred],
            []));

        Assert.True(result.Cancelled);
        Assert.Empty(result.FileBytes);
        Assert.NotEmpty(result.Issues);
        writer.VerifyNoOtherCalls();
    }
}

