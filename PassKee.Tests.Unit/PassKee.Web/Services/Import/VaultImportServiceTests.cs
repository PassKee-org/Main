using Moq;
using PassKee.Api.Shared.Models.Vaults;
using PassKee.Api.Shared.Models.Vaults.Payloads;
using PassKee.Web.Models.Vaults;
using PassKee.Web.Services.Import;
using PassKee.Web.Services.Vaults;

namespace PassKee.Tests.Unit.Web.Services.Import;

public class VaultImportServiceTests
{
    [Fact]
    public async Task ImportsHierarchyAndReusesExistingTag()
    {
        var vaultId = Guid.NewGuid();
        var parentId = Guid.NewGuid();
        var rootId = Guid.NewGuid();
        var tagId = Guid.NewGuid();
        var payload = new LoginCredentialPayload { Title = "Example", Password = "secret" };
        var root = new ImportGroup("Root", [], [new ImportGroup("Child", [new ImportEntry(payload, ["work"], [])], [])]);
        var reader = new Mock<IKdbxImportReader>();
        reader.Setup(service => service.ReadAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(root);
        var client = new Mock<IVaultClientService>();
        client.Setup(service => service.CreateDirectoryAsync(vaultId, parentId, "Root", It.IsAny<byte[]>())).ReturnsAsync(new DirectoryDto { Id = rootId });
        client.Setup(service => service.CreateDirectoryAsync(vaultId, rootId, "Child", It.IsAny<byte[]>())).ReturnsAsync(new DirectoryDto { Id = Guid.NewGuid() });
        client.Setup(service => service.CreateCredentialAsync(vaultId, It.IsAny<Guid?>(), It.IsAny<PassKee.Business.Common.Constants.CredentialType>(), payload, It.IsAny<byte[]>())).ReturnsAsync(new CredentialDto());
        var progress = new List<ImportProgress>();
        var result = await new VaultImportService(client.Object, reader.Object).ImportAsync(
            new VaultImportRequest(Stream.Null, "fixture", vaultId, parentId, new byte[32], [new DecryptedTag(tagId, vaultId, "Work")]), progress.Add);
        Assert.Equal(2, result.Directories);
        Assert.Equal(1, result.Entries);
        Assert.Equal([tagId], payload.TagIds);
        Assert.Equal(100, progress[^1].Percent);
        Assert.True(progress.Zip(progress.Skip(1)).All(pair => pair.First.Percent <= pair.Second.Percent));
        client.Verify(service => service.CreateTagAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<byte[]>()), Times.Never);
    }

    [Fact]
    public async Task ReaderFailureDoesNotWrite()
    {
        var reader = new Mock<IKdbxImportReader>();
        reader.Setup(service => service.ReadAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ThrowsAsync(new InvalidDataException());
        var client = new Mock<IVaultClientService>(MockBehavior.Strict);
        await Assert.ThrowsAsync<InvalidDataException>(() => new VaultImportService(client.Object, reader.Object)
            .ImportAsync(new VaultImportRequest(Stream.Null, "wrong", Guid.NewGuid(), null, new byte[32], [])));
        client.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ImportsFilesAndCustomFieldsAndCreatesTagOnce()
    {
        var vaultId = Guid.NewGuid();
        var fileId = Guid.NewGuid();
        var tagId = Guid.NewGuid();
        byte[]? uploadedBytes = null;
        var payload = new LoginCredentialPayload
        {
            Title = "Example",
            AdditionalFields = [new CredentialField { Label = "Custom", Value = "value", Type = CredentialFieldType.Text }]
        };
        var root = new ImportGroup("Root", [new ImportEntry(payload, ["Work", "work"], [new ImportAttachment("sample", [1, 2, 3])])], []);
        var client = Client();
        client.Setup(service => service.CreateTagAsync(vaultId, "Work", It.IsAny<byte[]>())).ReturnsAsync(new DecryptedTag(tagId, vaultId, "Work"));
        client.Setup(service => service.UploadFileAsync(vaultId, It.IsAny<byte[]>(), "file.bin", It.IsAny<byte[]>()))
            .Callback<Guid, byte[], string, byte[]>((id, data, name, key) => uploadedBytes = data.ToArray())
            .ReturnsAsync(new PassKee.Api.Shared.Models.Storage.StoredFileDto { Id = fileId });
        var result = await Service(client, root).ImportAsync(Request(vaultId));
        Assert.Equal(1, result.Entries);
        Assert.Equal(1, result.Files);
        Assert.Equal(1, result.Tags);
        Assert.Equal(new byte[] { 1, 2, 3 }, uploadedBytes);
        Assert.Equal(new[] { tagId }, payload.TagIds);
        Assert.Contains(payload.AdditionalFields, field => field.Label == "Custom" && field.Value == "value");
        Assert.Contains(payload.AdditionalFields, field => field.Type == CredentialFieldType.File && field.File?.Id == fileId && field.File.FileName == "sample");
        client.Verify(service => service.CreateTagAsync(vaultId, "Work", It.IsAny<byte[]>()), Times.Once);
    }

    [Fact]
    public async Task FailedAttachmentsAndTagsProducePartialEntryAndContinue()
    {
        var payload = new LoginCredentialPayload { Title = "Partial" };
        var root = new ImportGroup("Root", [new ImportEntry(payload, ["Missing"],
            [new ImportAttachment("Too large", new byte[PassKee.Business.Common.Constants.FileStorageConstants.MaxFileSize + 1]), new ImportAttachment("Failed", [1])]),
            new ImportEntry(new LoginCredentialPayload { Title = "Complete" }, [], [])], []);
        var client = Client();
        var progress = new List<ImportProgress>();
        var result = await Service(client, root).ImportAsync(Request(Guid.NewGuid()), progress.Add);
        Assert.Equal(1, result.PartialEntries);
        Assert.Equal(1, result.Entries);
        Assert.Equal(3, result.Issues.Count);
        Assert.Equal(100, progress[^1].Percent);
        client.Verify(service => service.UploadFileAsync(It.IsAny<Guid>(), It.Is<byte[]>(data => data.Length > PassKee.Business.Common.Constants.FileStorageConstants.MaxFileSize), It.IsAny<string>(), It.IsAny<byte[]>()), Times.Never);
        client.Verify(service => service.UploadFileAsync(It.IsAny<Guid>(), It.IsAny<byte[]>(), "file.bin", It.IsAny<byte[]>()), Times.Once);
    }

    [Fact]
    public async Task FailedDirectorySkipsOnlyDependentSubtree()
    {
        var root = new ImportGroup("Root", [], [new ImportGroup("Failed", [Entry()], [new ImportGroup("Never created", [Entry()], [])]),
            new ImportGroup("Good", [Entry()], [])]);
        var client = Client();
        client.Setup(service => service.CreateDirectoryAsync(It.IsAny<Guid>(), It.IsAny<Guid?>(), "Failed", It.IsAny<byte[]>())).ReturnsAsync((DirectoryDto?)null);
        var progress = new List<ImportProgress>();
        var result = await Service(client, root).ImportAsync(Request(Guid.NewGuid()), progress.Add);
        Assert.Equal(1, result.Entries);
        Assert.Equal(2, result.SkippedEntries);
        Assert.Equal(100, progress[^1].Percent);
        client.Verify(service => service.CreateDirectoryAsync(It.IsAny<Guid>(), It.IsAny<Guid?>(), "Never created", It.IsAny<byte[]>()), Times.Never);
    }

    [Fact]
    public async Task CancellationRetainsWrittenDataAndCountsRemainingEntries()
    {
        using var cancellation = new CancellationTokenSource();
        var client = Client();
        var root = new ImportGroup("Root", [Entry(), Entry()], []);
        client.Setup(service => service.CreateCredentialAsync(It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<PassKee.Business.Common.Constants.CredentialType>(), It.IsAny<BaseCredentialPayload>(), It.IsAny<byte[]>()))
            .Callback(() => cancellation.Cancel()).ReturnsAsync(new CredentialDto());
        var result = await Service(client, root).ImportAsync(Request(Guid.NewGuid()), cancellationToken: cancellation.Token);
        Assert.True(result.Cancelled);
        Assert.True(result.HasWrites);
        Assert.Equal(1, result.Entries);
        Assert.Equal(1, result.SkippedEntries);
        client.Verify(service => service.CreateCredentialAsync(It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<PassKee.Business.Common.Constants.CredentialType>(), It.IsAny<BaseCredentialPayload>(), It.IsAny<byte[]>()), Times.Once);
    }

    [Theory]
    [InlineData(System.Net.HttpStatusCode.BadRequest, true)]
    [InlineData(System.Net.HttpStatusCode.InternalServerError, false)]
    public async Task DoesNotRetryUnconfirmedWritesAndCleansUpOnlyDefiniteRejection(System.Net.HttpStatusCode status, bool cleanup)
    {
        var fileId = Guid.NewGuid();
        var client = Client();
        client.Setup(service => service.UploadFileAsync(It.IsAny<Guid>(), It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<byte[]>()))
            .ReturnsAsync(new PassKee.Api.Shared.Models.Storage.StoredFileDto { Id = fileId });
        client.Setup(service => service.CreateCredentialAsync(It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<PassKee.Business.Common.Constants.CredentialType>(), It.IsAny<BaseCredentialPayload>(), It.IsAny<byte[]>()))
            .ThrowsAsync(new HttpRequestException("failure", null, status));
        client.Setup(service => service.DeleteFileAsync(fileId)).ReturnsAsync(true);
        var root = new ImportGroup("Root", [new ImportEntry(new LoginCredentialPayload { Title = "Entry" }, [], [new ImportAttachment("sample", [1])])], []);
        var result = await Service(client, root).ImportAsync(Request(Guid.NewGuid()));
        Assert.Equal(1, result.SkippedEntries);
        client.Verify(service => service.DeleteFileAsync(fileId), cleanup ? Times.Once() : Times.Never());
        client.Verify(service => service.CreateCredentialAsync(It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<PassKee.Business.Common.Constants.CredentialType>(), It.IsAny<BaseCredentialPayload>(), It.IsAny<byte[]>()), Times.Once);
    }

    [Fact]
    public async Task LostAccessStopsInsteadOfContinuing()
    {
        var client = Client();
        client.Setup(service => service.CreateDirectoryAsync(It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<string>(), It.IsAny<byte[]>()))
            .ThrowsAsync(new HttpRequestException("unauthorized", null, System.Net.HttpStatusCode.Unauthorized));
        var result = await Service(client, new ImportGroup("Root", [Entry()], [])).ImportAsync(Request(Guid.NewGuid()));
        Assert.True(result.Cancelled);
        Assert.False(result.HasWrites);
        Assert.Equal(1, result.SkippedEntries);
        Assert.NotEmpty(result.Issues);
    }

    private static ImportEntry Entry() => new(new LoginCredentialPayload { Title = "Example" }, [], []);
    private static VaultImportRequest Request(Guid vaultId) => new(Stream.Null, "fixture", vaultId, null, new byte[32], []);

    private static Mock<IVaultClientService> Client()
    {
        var client = new Mock<IVaultClientService>();
        client.Setup(service => service.CreateDirectoryAsync(It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<string>(), It.IsAny<byte[]>())).ReturnsAsync(new DirectoryDto { Id = Guid.NewGuid() });
        client.Setup(service => service.CreateCredentialAsync(It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<PassKee.Business.Common.Constants.CredentialType>(), It.IsAny<BaseCredentialPayload>(), It.IsAny<byte[]>())).ReturnsAsync(new CredentialDto());
        return client;
    }

    private static VaultImportService Service(Mock<IVaultClientService> client, ImportGroup root)
    {
        var reader = new Mock<IKdbxImportReader>();
        reader.Setup(service => service.ReadAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(root);
        return new VaultImportService(client.Object, reader.Object);
    }
}