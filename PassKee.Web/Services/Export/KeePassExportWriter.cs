using DgNet.Keepass;

namespace PassKee.Web.Services.Export;

public sealed class KeePassExportWriter(Settings? settings = null) : IKdbxExportWriter
{
    private static readonly HashSet<string> StandardFieldKeys = new(StringComparer.Ordinal)
    {
        "Title",
        "UserName",
        "Password",
        "URL",
        "Notes"
    };

    public Task<byte[]> WriteAsync(ExportGroup rootGroup, string password, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var dbSettings = settings != null
            ? new Settings
            {
                Format = settings.Format,
                Cipher = settings.Cipher,
                IsCompressed = settings.IsCompressed,
                InnerStreamAlgorithm = settings.InnerStreamAlgorithm,
                Kdf = settings.Kdf
            }
            : new Settings();

        using var database = Database.Create(password, dbSettings);
        var rootName = string.IsNullOrWhiteSpace(rootGroup.Name) ? "PassKee Backup" : rootGroup.Name;
        database.Metadata.Name = rootName;
        database.Metadata.RecycleBinEnabled = false;
        database.Metadata.RecycleBinUuid = Guid.Empty;
        database.RootGroup.Name = rootName;

        PopulateGroup(database.RootGroup, rootGroup, cancellationToken);

        cancellationToken.ThrowIfCancellationRequested();
        using var output = new MemoryStream();
        new KdbxWriter(database).WriteTo(output);
        return Task.FromResult(output.ToArray());
    }

    private static void PopulateGroup(Group target, ExportGroup source, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        foreach (var entry in source.Entries)
        {
            cancellationToken.ThrowIfCancellationRequested();
            target.AddEntry(BuildEntry(entry));
        }

        foreach (var child in source.Groups)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var childGroup = new Group
            {
                Name = string.IsNullOrWhiteSpace(child.Name) ? "Unnamed group" : child.Name
            };
            target.AddGroup(childGroup);
            PopulateGroup(childGroup, child, cancellationToken);
        }
    }

    private static Entry BuildEntry(ExportEntry source)
    {
        var entry = new Entry
        {
            Title = string.IsNullOrWhiteSpace(source.Title) ? "Untitled entry" : source.Title,
            UserName = source.Username ?? string.Empty,
            Password = source.Password ?? string.Empty,
            Url = source.Url ?? string.Empty,
            Notes = source.Notes ?? string.Empty,
            Tags = string.Join("; ", source.Tags
                .Where(tag => !string.IsNullOrWhiteSpace(tag))
                .Select(tag => tag.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase))
        };

        foreach (var field in source.CustomFields)
        {
            var baseKey = string.IsNullOrWhiteSpace(field.Label) ? "Field" : field.Label.Trim();
            if (StandardFieldKeys.Contains(baseKey) &&
                !string.Equals(baseKey, "Title", StringComparison.Ordinal) &&
                entry.Strings.TryGetValue(baseKey, out var existingStandard) &&
                string.IsNullOrEmpty(existingStandard.Value))
            {
                entry.Strings[baseKey] = new EntryString
                {
                    Value = field.Value ?? string.Empty,
                    Protected = field.IsProtected || existingStandard.Protected
                };
                continue;
            }

            var key = GetUniqueStringKey(entry.Strings, baseKey);
            entry.Strings[key] = new EntryString
            {
                Value = field.Value ?? string.Empty,
                Protected = field.IsProtected
            };
        }

        var usedBinaryNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var attachment in source.Attachments)
        {
            var baseName = string.IsNullOrWhiteSpace(attachment.Name) ? "attachment.bin" : attachment.Name.Trim();
            var name = GetUniqueBinaryName(usedBinaryNames, baseName);
            entry.Binaries.Add(new EntryBinary
            {
                Name = name,
                Data = attachment.Data.ToArray(),
                IsProtected = false
            });
        }

        return entry;
    }

    private static string GetUniqueStringKey(Dictionary<string, EntryString> strings, string baseKey)
    {
        if (!strings.ContainsKey(baseKey))
        {
            return baseKey;
        }

        var suffix = 2;
        while (true)
        {
            var candidate = $"{baseKey} ({suffix})";
            if (!strings.ContainsKey(candidate))
            {
                return candidate;
            }
            suffix++;
        }
    }

    private static string GetUniqueBinaryName(HashSet<string> usedNames, string baseName)
    {
        if (usedNames.Add(baseName))
        {
            return baseName;
        }

        var extension = Path.GetExtension(baseName);
        var stem = Path.GetFileNameWithoutExtension(baseName);
        if (string.IsNullOrWhiteSpace(stem))
        {
            stem = "attachment";
        }

        var suffix = 2;
        while (true)
        {
            var candidate = $"{stem} ({suffix}){extension}";
            if (usedNames.Add(candidate))
            {
                return candidate;
            }
            suffix++;
        }
    }
}

