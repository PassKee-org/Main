using DgNet.Keepass;
using PassKee.Api.Shared.Models.Vaults.Payloads;
using PassKee.Web.Services.Import;

namespace PassKee.Tests.Unit.Web.Services.Import;

public class KeePassImportReaderTests
{
    [Fact]
    public void RecognizesKeePassAesKdfUuidInKdbx4Header()
    {
        var standardUuid = new byte[] { 0xc9, 0xd9, 0xf3, 0x9a, 0x62, 0x8a, 0x44, 0x60, 0xbf, 0x74, 0x0d, 0x08, 0xc1, 0x8a, 0x4f, 0xea };
        using var parameters = new MemoryStream();
        using (var writer = new BinaryWriter(parameters, System.Text.Encoding.UTF8, leaveOpen: true))
        {
            writer.Write((ushort)0x0100);
            writer.Write((byte)0x42);
            writer.Write(5);
            writer.Write(System.Text.Encoding.UTF8.GetBytes("$UUID"));
            writer.Write(standardUuid.Length);
            writer.Write(standardUuid);
            writer.Write((byte)0x42);
            writer.Write(1);
            writer.Write(System.Text.Encoding.UTF8.GetBytes("S"));
            writer.Write(32);
            writer.Write(new byte[32]);
            writer.Write((byte)0x05);
            writer.Write(1);
            writer.Write(System.Text.Encoding.UTF8.GetBytes("R"));
            writer.Write(8);
            writer.Write((ulong)100);
            writer.Write((byte)0);
        }
        using var stream = new MemoryStream();
        using (var writer = new BinaryWriter(stream, System.Text.Encoding.UTF8, leaveOpen: true))
        {
            writer.Write(0x9AA2D903u);
            writer.Write(0xB54BFB67u);
            writer.Write(0x00040000u);
            writer.Write((byte)11);
            writer.Write((uint)parameters.Length);
            writer.Write(parameters.ToArray());
            writer.Write((byte)0);
            writer.Write(0u);
        }
        stream.Position = 0;
        using var reader = new BinaryReader(stream);
        var kdf = Assert.IsType<AesKdf>(KdbxHeader.Read(reader).CreateKdf());
        Assert.Equal((ulong)100, kdf.Rounds);
        Assert.Equal(new byte[32], kdf.Seed);
    }

    [Theory]
    [InlineData(KdbxFormat.Kdbx3, CipherAlgorithm.Aes256Cbc, false)]
    [InlineData(KdbxFormat.Kdbx3, CipherAlgorithm.ChaCha20, false)]
    [InlineData(KdbxFormat.Kdbx4, CipherAlgorithm.Aes256Cbc, false)]
    [InlineData(KdbxFormat.Kdbx4, CipherAlgorithm.ChaCha20, false)]
    [InlineData(KdbxFormat.Kdbx4, CipherAlgorithm.Aes256Cbc, true)]
    public async Task ReadsHierarchyProtectedFieldsAttachmentsTagsAndOmitsRecycleBin(KdbxFormat format, CipherAlgorithm cipher, bool argon)
    {
        var settings = new Settings
        {
            Format = format,
            Cipher = cipher,
            InnerStreamAlgorithm = format == KdbxFormat.Kdbx3 ? ProtectedStreamAlgorithm.Salsa20 : ProtectedStreamAlgorithm.ChaCha20,
            Kdf = argon ? new Argon2Kdf(new byte[32], 1, 1024, 2, Argon2Type.D) : new AesKdf(new byte[32], 100)
        };
        using var database = Database.Create("fixture", settings);
        database.RootGroup.Name = "Source root";
        var child = new Group { Name = "Child" };
        database.RootGroup.AddGroup(child);
        child.AddGroup(new Group { Name = "Empty" });
        var entry = new Entry { Title = "Example", UserName = "alice", Password = "sensitive", Url = "https://example.com", Notes = "line1\nline2", Tags = "Work;work, Personal" };
        entry.Strings.Add("Custom secret", new EntryString { Value = "private", Protected = true });
        entry.Strings.Add("Empty attribute", new EntryString { Value = "" });
        entry.Binaries.Add(new EntryBinary { Name = "sample.txt", Data = [1, 2, 3] });
        entry.History.Add(new Entry { Title = "Old version", Password = "old" });
        child.AddEntry(entry);
        var recycleBin = new Group { Name = "Recycle Bin" };
        recycleBin.AddEntry(new Entry { Title = "Deleted" });
        database.RootGroup.AddGroup(recycleBin);
        database.Metadata.RecycleBinUuid = recycleBin.Uuid;
        using var stream = new MemoryStream();
        new KdbxWriter(database).WriteTo(stream);
        stream.Position = 0;

        var imported = await new KeePassImportReader().ReadAsync(stream, "fixture");
        Assert.Equal("Source root", imported.Name);
        Assert.Single(imported.Groups);
        var item = Assert.Single(imported.AllEntries);
        Assert.Equal("sensitive", item.Payload.Password);
        Assert.Equal("alice", item.Payload.Username);
        Assert.Equal("https://example.com", item.Payload.Website);
        Assert.Equal("line1\nline2", item.Payload.Notes);
        Assert.Contains(item.Payload.AdditionalFields, field => field.Label == "Custom secret" && field.Value == "private" && field.Type == CredentialFieldType.Password);
        Assert.Contains(item.Payload.AdditionalFields, field => field.Label == "Empty attribute" && field.Value == "");
        Assert.Equal(new[] { "Work", "Personal" }, item.Tags);
        Assert.Equal(new byte[] { 1, 2, 3 }, Assert.Single(item.Attachments).Data);
        Assert.Equal("Empty", Assert.Single(imported.Groups[0].Groups).Name);
    }

    [Theory]
    [InlineData("SimplePasswordV4.kdbx")]
    [InlineData("SimplePasswordV3_ChaCha20.kdbx")]
    public async Task ReadsUpstreamRealDatabase(string filename)
    {
        await using var stream = File.OpenRead(Path.Combine(AppContext.BaseDirectory, "Fixtures", filename));
        var imported = await new KeePassImportReader().ReadAsync(stream, "password123");
        Assert.NotEmpty(imported.AllEntries);
        Assert.Contains(imported.AllEntries, entry => !string.IsNullOrEmpty(entry.Payload.Password));
    }

    [Fact]
    public async Task ReadsUserCreatedDatabaseAndPreservesItsContents()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Fixtures", "test.kdbx");
        using var database = Database.Open(path, "456654");
        Assert.Equal((ushort)4, database.Version.Major);
        Assert.Equal(Argon2Type.D, Assert.IsType<Argon2Kdf>(database.Settings.Kdf).Type);

        await using var stream = File.OpenRead(path);
        var imported = await new KeePassImportReader().ReadAsync(stream, "456654");
        Assert.NotEmpty(imported.AllEntries);
        AssertImportedGroup(database.RootGroup, imported, database.Metadata.RecycleBinUuid);
    }

    [Fact]
    public async Task RejectsIncorrectPasswordForUserCreatedDatabase()
    {
        await using var stream = File.OpenRead(Path.Combine(AppContext.BaseDirectory, "Fixtures", "test.kdbx"));
        await Assert.ThrowsAsync<InvalidDataException>(() => new KeePassImportReader().ReadAsync(stream, "incorrect-password"));
    }

    private static void AssertImportedGroup(Group source, ImportGroup imported, Guid recycleBinId)
    {
        Assert.Equal(string.IsNullOrEmpty(source.Name) ? "Unnamed group" : source.Name, imported.Name);
        Assert.Equal(source.Entries.Count, imported.Entries.Count);
        foreach (var (expected, actual) in source.Entries.Zip(imported.Entries))
        {
            Assert.Equal(string.IsNullOrWhiteSpace(expected.Title) ? "Untitled entry" : expected.Title, actual.Payload.Title);
            Assert.Equal(expected.UserName, actual.Payload.Username);
            Assert.Equal(expected.Password, actual.Payload.Password);
            Assert.Equal(expected.Url, actual.Payload.Website);
            Assert.Equal(expected.Notes, actual.Payload.Notes);
            Assert.Equal(expected.Tags.Split([';', ','], StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                .Distinct(StringComparer.OrdinalIgnoreCase), actual.Tags);

            string[] standardFields = ["Title", "UserName", "Password", "URL", "Notes"];
            foreach (var (name, field) in expected.Strings.Where(field => !standardFields.Contains(field.Key, StringComparer.Ordinal)))
            {
                Assert.Contains(actual.Payload.AdditionalFields, importedField => importedField.Label == name &&
                    importedField.Value == field.Value &&
                    importedField.Type == (field.Protected ? CredentialFieldType.Password : CredentialFieldType.Text));
            }

            Assert.Equal(expected.Binaries.Count, actual.Attachments.Count);
            foreach (var (expectedFile, actualFile) in expected.Binaries.Zip(actual.Attachments))
            {
                Assert.Equal(expectedFile.Name, actualFile.Name);
                Assert.Equal(expectedFile.Data, actualFile.Data);
            }
        }

        var groups = source.Groups.Where(group => recycleBinId == Guid.Empty || group.Uuid != recycleBinId).ToList();
        Assert.Equal(groups.Count, imported.Groups.Count);
        foreach (var (expected, actual) in groups.Zip(imported.Groups))
            AssertImportedGroup(expected, actual, recycleBinId);
    }

    [Fact]
    public async Task RejectsWrongPasswordAndTamperedFile()
    {
        using var database = Database.Create("fixture", new Settings { Kdf = new AesKdf(new byte[32], 100) });
        using var stream = new MemoryStream();
        new KdbxWriter(database).WriteTo(stream);
        var bytes = stream.ToArray();
        await Assert.ThrowsAnyAsync<Exception>(() => new KeePassImportReader().ReadAsync(new MemoryStream(bytes), "wrong"));
        bytes[^16] ^= 1;
        await Assert.ThrowsAnyAsync<Exception>(() => new KeePassImportReader().ReadAsync(new MemoryStream(bytes), "fixture"));
        await Assert.ThrowsAnyAsync<Exception>(() => new KeePassImportReader().ReadAsync(new MemoryStream(bytes[..50]), "fixture"));
    }

    [Fact]
    public void RejectsExcessiveKdfBeforeAllocatingMemory()
    {
        Assert.Throws<NotSupportedException>(() => new Argon2Kdf(new byte[32], 1, int.MaxValue, 2).Transform(new byte[32]));
        Assert.Throws<NotSupportedException>(() => new AesKdf(new byte[32], ulong.MaxValue).Transform(new byte[32]));
    }
}
