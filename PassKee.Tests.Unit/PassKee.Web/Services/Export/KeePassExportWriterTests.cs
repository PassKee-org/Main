using DgNet.Keepass;
using PassKee.Api.Shared.Models.Vaults.Payloads;
using PassKee.Web.Services.Export;
using PassKee.Web.Services.Import;

namespace PassKee.Tests.Unit.Web.Services.Export;

public class KeePassExportWriterTests
{
    [Theory]
    [InlineData(KdbxFormat.Kdbx3, CipherAlgorithm.Aes256Cbc, false)]
    [InlineData(KdbxFormat.Kdbx4, CipherAlgorithm.Aes256Cbc, false)]
    [InlineData(KdbxFormat.Kdbx4, CipherAlgorithm.ChaCha20, true)]
    public async Task WritesHierarchyProtectedFieldsAttachmentsTagsAndRoundTripsThroughReader(
        KdbxFormat format,
        CipherAlgorithm cipher,
        bool argon)
    {
        var settings = new Settings
        {
            Format = format,
            Cipher = cipher,
            InnerStreamAlgorithm = format == KdbxFormat.Kdbx3 ? ProtectedStreamAlgorithm.Salsa20 : ProtectedStreamAlgorithm.ChaCha20,
            Kdf = argon ? new Argon2Kdf(new byte[32], 1, 1024, 2, Argon2Type.Id) : new AesKdf(new byte[32], 100)
        };

        var rootEntry = new ExportEntry(
            "Root Login",
            "bob",
            "root-pass",
            "https://root.example.com",
            "Root note",
            ["RootTag"],
            [],
            []);

        var childEntry = new ExportEntry(
            "Example",
            "alice",
            "sensitive",
            "https://example.com",
            "line1\nline2",
            ["Work", "work", "Personal"],
            [
                new ExportField("Custom secret", "private", IsProtected: true),
                new ExportField("Empty attribute", "", IsProtected: false)
            ],
            [
                new ExportAttachment("sample.txt", [1, 2, 3])
            ]);

        var archivedEntry = new ExportEntry(
            "Old Account",
            "legacy",
            "legacy-secret",
            "https://legacy.example.com",
            "Archived item",
            ["Old"],
            [],
            []);

        var rootGroup = new ExportGroup(
            "Personal Vault",
            [rootEntry],
            [
                new ExportGroup("Child", [childEntry], [new ExportGroup("Empty", [], [])]),
                new ExportGroup(VaultExportService.ArchiveDirectoryName, [archivedEntry], [])
            ]);

        var bytes = await new KeePassExportWriter(settings).WriteAsync(rootGroup, "backup-password");
        Assert.NotEmpty(bytes);

        await using var stream = new MemoryStream(bytes);
        var imported = await new KeePassImportReader().ReadAsync(stream, "backup-password");

        Assert.Equal("Personal Vault", imported.Name);
        var importedRootEntry = Assert.Single(imported.Entries);
        Assert.Equal("Root Login", importedRootEntry.Payload.Title);
        Assert.Equal("root-pass", importedRootEntry.Payload.Password);

        Assert.Equal(2, imported.Groups.Count);
        var importedChildGroup = imported.Groups.Single(g => g.Name == "Child");
        var importedItem = Assert.Single(importedChildGroup.Entries);
        Assert.Equal("sensitive", importedItem.Payload.Password);
        Assert.Equal("alice", importedItem.Payload.Username);
        Assert.Equal("https://example.com", importedItem.Payload.Website);
        Assert.Equal("line1\nline2", importedItem.Payload.Notes);
        Assert.Contains(importedItem.Payload.AdditionalFields, field =>
            field.Label == "Custom secret" &&
            field.Value == "private" &&
            field.Type == CredentialFieldType.Password);
        Assert.Contains(importedItem.Payload.AdditionalFields, field =>
            field.Label == "Empty attribute" &&
            field.Value == "");
        Assert.Equal(new[] { "Work", "Personal" }, importedItem.Tags);
        Assert.Equal(new byte[] { 1, 2, 3 }, Assert.Single(importedItem.Attachments).Data);
        Assert.Equal("Empty", Assert.Single(importedChildGroup.Groups).Name);

        var importedArchiveGroup = imported.Groups.Single(g => g.Name == VaultExportService.ArchiveDirectoryName);
        var importedArchivedItem = Assert.Single(importedArchiveGroup.Entries);
        Assert.Equal("Old Account", importedArchivedItem.Payload.Title);
        Assert.Equal("legacy-secret", importedArchivedItem.Payload.Password);
    }

    [Fact]
    public async Task DisambiguatesDuplicateFieldLabelsAndAttachmentNames()
    {
        var settings = new Settings { Kdf = new AesKdf(new byte[32], 100) };
        var entry = new ExportEntry(
            "Entry",
            "user",
            "main-pass",
            "",
            "",
            [],
            [
                new ExportField("Password", "secondary-pass", IsProtected: true),
                new ExportField("API Key", "key-1", IsProtected: true),
                new ExportField("API Key", "key-2", IsProtected: true)
            ],
            [
                new ExportAttachment("doc.txt", [1]),
                new ExportAttachment("doc.txt", [2])
            ]);

        var rootGroup = new ExportGroup("Vault", [entry], []);
        var bytes = await new KeePassExportWriter(settings).WriteAsync(rootGroup, "pass");

        await using var stream = new MemoryStream(bytes);
        var imported = await new KeePassImportReader().ReadAsync(stream, "pass");
        var importedEntry = Assert.Single(imported.Entries);

        Assert.Equal("main-pass", importedEntry.Payload.Password);
        Assert.Contains(importedEntry.Payload.AdditionalFields, f => f.Label == "Password (2)" && f.Value == "secondary-pass");
        Assert.Contains(importedEntry.Payload.AdditionalFields, f => f.Label == "API Key" && f.Value == "key-1");
        Assert.Contains(importedEntry.Payload.AdditionalFields, f => f.Label == "API Key (2)" && f.Value == "key-2");
        Assert.Equal(2, importedEntry.Attachments.Count);
        Assert.Contains(importedEntry.Attachments, a => a.Name == "doc.txt" && a.Data.SequenceEqual(new byte[] { 1 }));
        Assert.Contains(importedEntry.Attachments, a => a.Name == "doc (2).txt" && a.Data.SequenceEqual(new byte[] { 2 }));
    }
}

