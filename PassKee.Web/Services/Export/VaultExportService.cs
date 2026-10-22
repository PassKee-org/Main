using System.Net;
using PassKee.Api.Shared.Models.Storage;
using PassKee.Api.Shared.Models.Vaults.Payloads;
using PassKee.Web.Models.Vaults;
using PassKee.Web.Services.Vaults;

namespace PassKee.Web.Services.Export;

public sealed class VaultExportService(IVaultClientService vaultClient, IKdbxExportWriter writer) : IVaultExportService
{
    public const string ArchiveDirectoryName = "Archive";

    private bool _running;

    public async Task<VaultExportResult> ExportAsync(VaultExportRequest request, Action<ExportProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        if (_running) throw new InvalidOperationException("An export is already running.");
        if (request.VaultKey.Length != 32) throw new InvalidOperationException("Vault is locked.");
        if (string.IsNullOrWhiteSpace(request.Password)) throw new ArgumentException("Database password is required.", nameof(request));

        _running = true;
        var key = request.VaultKey.ToArray();
        var run = new Run(vaultClient, writer, request with { VaultKey = key }, progress, cancellationToken);
        try
        {
            return await run.ExecuteAsync();
        }
        catch (OperationCanceledException)
        {
            run.Stop();
            return run.BuildResult([]);
        }
        catch (HttpRequestException exception) when (exception.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
        {
            run.Stop();
            run.Issues.Add(new ExportIssue("Export", "Access was lost during backup."));
            return run.BuildResult([]);
        }
        finally
        {
            Array.Clear(key);
            run.ClearAttachments();
            _running = false;
        }
    }

    private sealed class Run(
        IVaultClientService client,
        IKdbxExportWriter writer,
        VaultExportRequest request,
        Action<ExportProgress>? progress,
        CancellationToken cancellationToken)
    {
        private readonly Dictionary<Guid, string> _tags = new();
        private readonly List<byte[]> _downloadedBuffers = [];
        private int _total;
        private int _completed;
        private int _directories;
        private int _entries;
        private int _archivedEntries;
        private int _partialEntries;
        private int _files;

        public bool Cancelled { get; private set; }
        public List<ExportIssue> Issues { get; } = [];

        public void Stop()
        {
            Cancelled = true;
        }

        public void ClearAttachments()
        {
            foreach (var buffer in _downloadedBuffers)
            {
                Array.Clear(buffer);
            }
            _downloadedBuffers.Clear();
        }

        public VaultExportResult BuildResult(byte[] fileBytes) => new()
        {
            FileName = BuildFileName(request.VaultName),
            FileBytes = Cancelled ? [] : fileBytes,
            Directories = _directories,
            Entries = _entries,
            ArchivedEntries = _archivedEntries,
            PartialEntries = _partialEntries,
            Files = _files,
            Cancelled = Cancelled,
            Issues = Issues.ToArray()
        };

        public async Task<VaultExportResult> ExecuteAsync()
        {
            cancellationToken.ThrowIfCancellationRequested();
            progress?.Invoke(new ExportProgress(0, 0, "Loading archived entries"));

            foreach (var tag in request.Tags.Where(t => t.VaultId == request.VaultId && !string.IsNullOrWhiteSpace(t.Name)))
            {
                _tags[tag.Id] = tag.Name.Trim();
            }

            var archivedList = await LoadArchivedCredentialsAsync();
            var activeList = request.Credentials
                .Where(c => !c.ArchivedAt.HasValue)
                .OrderBy(c => c.Order)
                .ToList();

            var directoryMap = request.Directories.ToDictionary(d => d.Id, d => d);
            var childrenMap = new Dictionary<Guid, List<DecryptedDirectory>>();
            var rootDirectories = new List<DecryptedDirectory>();

            foreach (var dir in request.Directories)
            {
                if (dir.ParentDirectoryId.HasValue && directoryMap.ContainsKey(dir.ParentDirectoryId.Value))
                {
                    if (!childrenMap.TryGetValue(dir.ParentDirectoryId.Value, out var children))
                    {
                        children = [];
                        childrenMap[dir.ParentDirectoryId.Value] = children;
                    }
                    children.Add(dir);
                }
                else
                {
                    rootDirectories.Add(dir);
                }
            }

            var reachableDirectories = new HashSet<Guid>();
            foreach (var rootDir in rootDirectories)
            {
                CollectReachableDirectories(rootDir.Id, childrenMap, reachableDirectories);
            }

            var credentialsByDirectory = new Dictionary<Guid, List<DecryptedCredential>>();
            var rootCredentials = new List<DecryptedCredential>();

            foreach (var cred in activeList)
            {
                if (cred.DirectoryId.HasValue && reachableDirectories.Contains(cred.DirectoryId.Value))
                {
                    if (!credentialsByDirectory.TryGetValue(cred.DirectoryId.Value, out var list))
                    {
                        list = [];
                        credentialsByDirectory[cred.DirectoryId.Value] = list;
                    }
                    list.Add(cred);
                }
                else
                {
                    rootCredentials.Add(cred);
                }
            }

            var totalAttachments = activeList.Sum(c => CollectFileReferences(c.Payload).Count)
                + archivedList.Sum(c => CollectFileReferences(c.Payload).Count);

            _total = reachableDirectories.Count
                + activeList.Count
                + archivedList.Count
                + totalAttachments
                + 1;

            var rootName = string.IsNullOrWhiteSpace(request.VaultName) ? "PassKee Backup" : request.VaultName.Trim();
            var rootEntries = new List<ExportEntry>();
            foreach (var cred in rootCredentials)
            {
                var entry = await ExportCredentialAsync(cred, $"{rootName}/{GetEntryTitle(cred.Payload)}", isArchived: false);
                rootEntries.Add(entry);
            }

            var exportedGroups = new List<ExportGroup>();
            foreach (var rootDir in rootDirectories)
            {
                var group = await ExportDirectoryAsync(rootDir, childrenMap, credentialsByDirectory, $"{rootName}/{rootDir.Name}");
                exportedGroups.Add(group);
            }

            if (archivedList.Count > 0)
            {
                var archivedEntries = new List<ExportEntry>();
                foreach (var archivedCred in archivedList)
                {
                    var entry = await ExportCredentialAsync(
                        archivedCred,
                        $"{rootName}/{ArchiveDirectoryName}/{GetEntryTitle(archivedCred.Payload)}",
                        isArchived: true);
                    archivedEntries.Add(entry);
                }

                exportedGroups.Add(new ExportGroup(ArchiveDirectoryName, archivedEntries, []));
            }

            cancellationToken.ThrowIfCancellationRequested();
            var rootGroup = new ExportGroup(rootName, rootEntries, exportedGroups);
            var fileBytes = await writer.WriteAsync(rootGroup, request.Password, cancellationToken);
            Advance("Writing database");

            return BuildResult(fileBytes);
        }

        private async Task<List<DecryptedCredential>> LoadArchivedCredentialsAsync()
        {
            var combined = new Dictionary<Guid, DecryptedCredential>();
            foreach (var cred in request.Credentials.Where(c => c.ArchivedAt.HasValue))
            {
                combined[cred.Id] = cred;
            }

            if (request.ArchivedCredentials != null)
            {
                foreach (var cred in request.ArchivedCredentials)
                {
                    combined[cred.Id] = cred;
                }
            }

            try
            {
                var fetched = await client.GetArchivedCredentialsAsync(request.VaultId, request.VaultKey);
                if (fetched != null)
                {
                    foreach (var cred in fetched)
                    {
                        combined[cred.Id] = cred;
                    }
                }
            }
            catch (Exception exception) when (CanContinue(exception))
            {
                Issues.Add(new ExportIssue(ArchiveDirectoryName, "Archived entries could not be loaded from server."));
            }

            return combined.Values.OrderBy(c => c.Order).ToList();
        }

        private static void CollectReachableDirectories(
            Guid directoryId,
            Dictionary<Guid, List<DecryptedDirectory>> childrenMap,
            HashSet<Guid> visited)
        {
            if (!visited.Add(directoryId))
            {
                return;
            }

            if (childrenMap.TryGetValue(directoryId, out var children))
            {
                foreach (var child in children)
                {
                    CollectReachableDirectories(child.Id, childrenMap, visited);
                }
            }
        }

        private async Task<ExportGroup> ExportDirectoryAsync(
            DecryptedDirectory directory,
            Dictionary<Guid, List<DecryptedDirectory>> childrenMap,
            Dictionary<Guid, List<DecryptedCredential>> credentialsByDirectory,
            string path)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _directories++;
            Advance("Exporting directories");

            var entries = new List<ExportEntry>();
            if (credentialsByDirectory.TryGetValue(directory.Id, out var credentials))
            {
                foreach (var cred in credentials)
                {
                    var exportedEntry = await ExportCredentialAsync(cred, $"{path}/{GetEntryTitle(cred.Payload)}", isArchived: false);
                    entries.Add(exportedEntry);
                }
            }

            var subGroups = new List<ExportGroup>();
            if (childrenMap.TryGetValue(directory.Id, out var children))
            {
                foreach (var child in children)
                {
                    var exportedChild = await ExportDirectoryAsync(child, childrenMap, credentialsByDirectory, $"{path}/{child.Name}");
                    subGroups.Add(exportedChild);
                }
            }

            return new ExportGroup(directory.Name, entries, subGroups);
        }

        private async Task<ExportEntry> ExportCredentialAsync(DecryptedCredential credential, string path, bool isArchived)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var issueCount = Issues.Count;
            var payload = credential.Payload;

            var (username, password, url, customFields) = ExtractFields(payload);
            var tags = payload.TagIds
                .Select(id => _tags.GetValueOrDefault(id))
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Select(name => name!)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var attachments = new List<ExportAttachment>();
            foreach (var (fileDto, fallbackName) in CollectFileReferences(payload))
            {
                cancellationToken.ThrowIfCancellationRequested();
                var attachmentName = !string.IsNullOrWhiteSpace(fileDto.FileName)
                    ? fileDto.FileName.Trim()
                    : (!string.IsNullOrWhiteSpace(fallbackName) ? fallbackName.Trim() : "attachment.bin");

                try
                {
                    var bytes = await client.DownloadFileAsync(fileDto.Id, request.VaultKey);
                    if (bytes == null)
                    {
                        Issues.Add(new ExportIssue(path, $"Attachment '{attachmentName}' could not be downloaded."));
                    }
                    else
                    {
                        _downloadedBuffers.Add(bytes);
                        attachments.Add(new ExportAttachment(attachmentName, bytes));
                        _files++;
                    }
                }
                catch (Exception exception) when (CanContinue(exception))
                {
                    Issues.Add(new ExportIssue(path, $"Attachment '{attachmentName}' failed to download."));
                }

                Advance("Downloading attachments");
            }

            if (Issues.Count == issueCount)
            {
                _entries++;
            }
            else
            {
                _partialEntries++;
            }

            if (isArchived)
            {
                _archivedEntries++;
            }

            Advance(isArchived ? "Exporting archived entries" : "Exporting entries");

            return new ExportEntry(
                GetEntryTitle(payload),
                username,
                password,
                url,
                payload.Notes ?? string.Empty,
                tags,
                customFields,
                attachments);
        }

        private static (string Username, string Password, string Url, List<ExportField> CustomFields) ExtractFields(BaseCredentialPayload payload)
        {
            var username = string.Empty;
            var password = string.Empty;
            var url = string.Empty;
            var customFields = new List<ExportField>();

            switch (payload)
            {
                case LoginCredentialPayload login:
                    username = login.Username ?? string.Empty;
                    password = login.Password ?? string.Empty;
                    url = login.Website ?? string.Empty;
                    break;
                case PasswordCredentialPayload pwd:
                    username = pwd.Username ?? string.Empty;
                    password = pwd.Password ?? string.Empty;
                    break;
                case ServerCredentialPayload server:
                    username = server.Username ?? string.Empty;
                    password = server.Password ?? string.Empty;
                    url = server.Url ?? string.Empty;
                    break;
                case CardCredentialPayload card:
                    if (!string.IsNullOrEmpty(card.CardNumber))
                        customFields.Add(new ExportField("Card Number", card.CardNumber, IsProtected: true));
                    if (!string.IsNullOrEmpty(card.CardholderName))
                        customFields.Add(new ExportField("Cardholder Name", card.CardholderName, IsProtected: false));
                    if (!string.IsNullOrEmpty(card.ExpirationDate))
                        customFields.Add(new ExportField("Expiration Date", card.ExpirationDate, IsProtected: false));
                    if (!string.IsNullOrEmpty(card.Cvv))
                        customFields.Add(new ExportField("CVV", card.Cvv, IsProtected: true));
                    if (!string.IsNullOrEmpty(card.Pin))
                        customFields.Add(new ExportField("PIN", card.Pin, IsProtected: true));
                    break;
            }

            foreach (var field in payload.AdditionalFields.Where(f => f.Type != CredentialFieldType.File))
            {
                var label = string.IsNullOrWhiteSpace(field.Label) ? "Field" : field.Label.Trim();
                customFields.Add(new ExportField(label, field.Value ?? string.Empty, IsProtectedField(field.Type)));
            }

            foreach (var section in payload.Sections)
            {
                var sectionTitle = section.Title?.Trim();
                foreach (var field in section.Fields.Where(f => f.Type != CredentialFieldType.File))
                {
                    var fieldLabel = string.IsNullOrWhiteSpace(field.Label) ? "Field" : field.Label.Trim();
                    var label = string.IsNullOrWhiteSpace(sectionTitle)
                        ? fieldLabel
                        : $"{sectionTitle}: {fieldLabel}";
                    customFields.Add(new ExportField(label, field.Value ?? string.Empty, IsProtectedField(field.Type)));
                }
            }

            return (username, password, url, customFields);
        }

        private static List<(StoredFileDto File, string FallbackName)> CollectFileReferences(BaseCredentialPayload payload)
        {
            var results = new List<(StoredFileDto File, string FallbackName)>();
            var seenIds = new HashSet<Guid>();

            void AddFile(StoredFileDto? file, string fallbackName)
            {
                if (file != null && file.Id != Guid.Empty && seenIds.Add(file.Id))
                {
                    results.Add((file, fallbackName));
                }
            }

            switch (payload)
            {
                case FileCredentialPayload filePayload:
                    AddFile(filePayload.File, filePayload.Title);
                    break;
                case SshKeyCredentialPayload sshPayload:
                    AddFile(sshPayload.PrivateKeyFile, "id_rsa");
                    break;
            }

            foreach (var field in payload.AdditionalFields.Where(f => f.Type == CredentialFieldType.File))
            {
                AddFile(field.File, field.Label);
            }

            foreach (var section in payload.Sections)
            {
                foreach (var field in section.Fields.Where(f => f.Type == CredentialFieldType.File))
                {
                    AddFile(field.File, field.Label);
                }
            }

            return results;
        }

        private static bool IsProtectedField(CredentialFieldType type) =>
            type is CredentialFieldType.Password or CredentialFieldType.OneTimePassword or CredentialFieldType.SecurityQuestion;

        private static string GetEntryTitle(BaseCredentialPayload payload) =>
            string.IsNullOrWhiteSpace(payload.Title) ? "Untitled entry" : payload.Title.Trim();

        private static string BuildFileName(string vaultName)
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            var sanitized = new string((vaultName ?? string.Empty)
                .Trim()
                .Select(ch => invalidChars.Contains(ch) || char.IsWhiteSpace(ch) ? '-' : ch)
                .ToArray())
                .Trim('-');

            if (string.IsNullOrWhiteSpace(sanitized))
            {
                sanitized = "vault";
            }

            return $"{sanitized}-backup-{DateTime.UtcNow:yyyy-MM-dd}.kdbx";
        }

        private void Advance(string phase, int count = 1)
        {
            _completed += count;
            progress?.Invoke(new ExportProgress(_completed, _total, phase));
        }

        private static bool CanContinue(Exception exception) => exception is not OperationCanceledException &&
            exception is not HttpRequestException { StatusCode: HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden };
    }
}

