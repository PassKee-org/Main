using DgNet.Keepass;
using PassKee.Api.Shared.Models.Vaults.Payloads;

namespace PassKee.Web.Services.Import;

public sealed class KeePassImportReader : IKdbxImportReader
{
    public async Task<ImportGroup> ReadAsync(Stream stream, string password, CancellationToken cancellationToken = default)
    {
        using var input = new MemoryStream();
        var buffer = new byte[81920];
        int count;
        while ((count = await stream.ReadAsync(buffer, cancellationToken)) > 0)
        {
            if (input.Length + count > ReadLimits.MaxFileBytes)
                throw new NotSupportedException("The database exceeds the 100 MB browser import limit.");
            await input.WriteAsync(buffer.AsMemory(0, count), cancellationToken);
        }

        cancellationToken.ThrowIfCancellationRequested();
        input.Position = 0;
        var inputBytes = input.GetBuffer();
        using var database = new Database(new CompositeKey(password));
        try
        {
            new KdbxReader(database).ReadFrom(input);
            cancellationToken.ThrowIfCancellationRequested();
            return ConvertGroup(database.RootGroup, database.Metadata.RecycleBinUuid);
        }
        finally
        {
            Array.Clear(buffer);
            Array.Clear(inputBytes);
        }
    }

    private static ImportGroup ConvertGroup(Group group, Guid recycleBinId) => new(
        string.IsNullOrEmpty(group.Name) ? "Unnamed group" : group.Name,
        group.Entries.Select(ConvertEntry).ToList(),
        group.Groups.Where(child => recycleBinId == Guid.Empty || child.Uuid != recycleBinId)
            .Select(child => ConvertGroup(child, recycleBinId)).ToList());

    private static ImportEntry ConvertEntry(Entry entry)
    {
        var payload = new LoginCredentialPayload
        {
            Title = string.IsNullOrWhiteSpace(entry.Title) ? "Untitled entry" : entry.Title,
            Username = entry.UserName,
            Password = entry.Password,
            Website = entry.Url,
            Notes = entry.Notes
        };
        string[] standardFields = ["Title", "UserName", "Password", "URL", "Notes"];
        foreach (var (name, field) in entry.Strings)
        {
            if (!standardFields.Contains(name, StringComparer.Ordinal))
                payload.AdditionalFields.Add(new CredentialField
                {
                    Label = name,
                    Value = field.Value,
                    Type = field.Protected ? CredentialFieldType.Password : CredentialFieldType.Text
                });
        }

        string[] resolved = [entry.Title, entry.UserName, entry.Password, entry.Url, entry.Notes];
        for (var index = 0; index < standardFields.Length; index++)
        {
            var name = standardFields[index];
            if (entry.Strings.TryGetValue(name, out var original) &&
                (original.Value != resolved[index] || (name == "Title" && string.IsNullOrWhiteSpace(original.Value))))
                payload.AdditionalFields.Add(new CredentialField
                {
                    Label = $"KeePass source {name}",
                    Value = original.Value,
                    Type = original.Protected ? CredentialFieldType.Password : CredentialFieldType.Text
                });
        }

        var tags = entry.Tags.Split([';', ','], StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        return new ImportEntry(payload, tags, entry.Binaries.Select(binary => new ImportAttachment(binary.Name, binary.Data)).ToList());
    }
}