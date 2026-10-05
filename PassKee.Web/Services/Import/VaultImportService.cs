using System.Net;
using PassKee.Api.Shared.Models.Vaults.Payloads;
using PassKee.Business.Common.Constants;
using PassKee.Web.Services.Vaults;

namespace PassKee.Web.Services.Import;

public sealed class VaultImportService(IVaultClientService vaultClient, IKdbxImportReader reader) : IVaultImportService
{
    private bool _running;

    public async Task<VaultImportResult> ImportAsync(VaultImportRequest request, Action<ImportProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        if (_running) throw new InvalidOperationException("An import is already running.");
        if (request.VaultKey.Length != 32) throw new InvalidOperationException("Vault is locked.");
        _running = true;
        var key = request.VaultKey.ToArray();
        ImportGroup? root = null;
        var result = new Run(vaultClient, request with { VaultKey = key }, progress, cancellationToken);
        try
        {
            progress?.Invoke(new ImportProgress(0, 0, "Opening database"));
            root = await reader.ReadAsync(request.DatabaseStream, request.Password, cancellationToken);
            await result.ExecuteAsync(root);
        }
        catch (OperationCanceledException)
        {
            result.Stop(root);
        }
        catch (HttpRequestException exception) when (exception.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
        {
            result.Stop(root);
            result.Issues.Add(new ImportIssue("Import", "Access was lost. Previously saved data is retained."));
        }
        finally
        {
            Array.Clear(key);
            if (root != null)
                foreach (var attachment in root.AllEntries.SelectMany(entry => entry.Attachments))
                    Array.Clear(attachment.Data);
            _running = false;
        }
        return result.Result;
    }

    private sealed class Run(IVaultClientService client, VaultImportRequest request, Action<ImportProgress>? progress, CancellationToken cancellationToken)
    {
        private readonly Dictionary<string, Guid?> _tags = new(StringComparer.OrdinalIgnoreCase);
        private int _total;
        private int _completed;
        private int _directories;
        private int _entries;
        private int _partialEntries;
        private int _skippedEntries;
        private int _files;
        private int _createdTags;
        private bool _hasWrites;
        private readonly HashSet<Guid> _unlinkedFiles = [];
        public bool Cancelled { get; set; }
        public List<ImportIssue> Issues { get; } = [];
        public VaultImportResult Result => new()
        {
            Directories = _directories, Entries = _entries, PartialEntries = _partialEntries,
            SkippedEntries = _skippedEntries, Files = _files, Tags = _createdTags,
            HasWrites = _hasWrites, Cancelled = Cancelled, Issues = Issues.ToArray()
        };

        public void Stop(ImportGroup? root)
        {
            Cancelled = true;
            if (root != null) _skippedEntries = root.AllEntries.Count() - _entries - _partialEntries;
            foreach (var fileId in _unlinkedFiles)
                Issues.Add(new ImportIssue("Import", $"File {fileId} may not be linked to an entry. Check storage before retrying."));
        }

        public async Task ExecuteAsync(ImportGroup root)
        {
            foreach (var tag in request.ExistingTags.Where(tag => tag.VaultId == request.VaultId))
                _tags.TryAdd(tag.Name.Trim(), tag.Id);
            var missingTags = root.AllEntries.SelectMany(entry => entry.Tags)
                .Distinct(StringComparer.OrdinalIgnoreCase).Where(name => !_tags.ContainsKey(name)).ToList();
            var totalWork = root.Entries.Sum(entry => 1 + entry.Attachments.Count) + root.Groups.Sum(group => group.WorkCount);
            _total = totalWork + missingTags.Count;
            foreach (var name in missingTags)
            {
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    var created = await client.CreateTagAsync(request.VaultId, name, request.VaultKey);
                    _tags[name] = created?.Id;
                    if (created != null) { _createdTags++; _hasWrites = true; }
                }
                catch (Exception exception) when (CanContinue(exception))
                {
                    _tags[name] = null;
                }
                Advance("Importing tags");
            }
            foreach (var entry in root.Entries)
                await ImportEntryAsync(entry, request.ParentDirectoryId, $"{root.Name}/{entry.Payload.Title}");
            foreach (var child in root.Groups)
                await ImportGroupAsync(child, request.ParentDirectoryId, $"{root.Name}/{child.Name}");
        }

        private async Task ImportGroupAsync(ImportGroup group, Guid? parentId, string path)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Guid? directoryId = null;
            try
            {
                directoryId = (await client.CreateDirectoryAsync(request.VaultId, parentId, group.Name, request.VaultKey))?.Id;
            }
            catch (Exception exception) when (CanContinue(exception)) { }
            Advance("Importing directories");
            if (!directoryId.HasValue)
            {
                Issues.Add(new ImportIssue(path, "Directory could not be saved; its dependent entries and groups were skipped."));
                _skippedEntries += group.AllEntries.Count();
                Advance("Skipping dependent items", group.WorkCount - 1);
                return;
            }
            _directories++;
            _hasWrites = true;
            foreach (var entry in group.Entries)
                await ImportEntryAsync(entry, directoryId.Value, $"{path}/{entry.Payload.Title}");
            foreach (var child in group.Groups)
                await ImportGroupAsync(child, directoryId, $"{path}/{child.Name}");
        }

        private async Task ImportEntryAsync(ImportEntry entry, Guid? directoryId, string path)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var issueCount = Issues.Count;
            var payload = entry.Payload;
            payload.TagIds = entry.Tags.Where(name => _tags.GetValueOrDefault(name).HasValue)
                .Select(name => _tags[name]!.Value).Distinct().ToList();
            foreach (var name in entry.Tags.Where(name => !_tags.GetValueOrDefault(name).HasValue))
                Issues.Add(new ImportIssue(path, $"Tag '{name}' could not be saved."));

            var uploaded = new List<Guid>();
            foreach (var attachment in entry.Attachments)
            {
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    if (attachment.Data.Length > FileStorageConstants.MaxFileSize)
                        Issues.Add(new ImportIssue(path, $"Attachment '{attachment.Name}' exceeds 20 MB and was skipped."));
                    else
                    {
                        var file = await client.UploadFileAsync(request.VaultId, attachment.Data, $"file.{FileStorageConstants.CloudFileExtension}", request.VaultKey);
                        if (file == null)
                            Issues.Add(new ImportIssue(path, $"Attachment '{attachment.Name}' could not be saved."));
                        else
                        {
                            file.FileName = attachment.Name;
                            _files++;
                            _hasWrites = true;
                            uploaded.Add(file.Id);
                            _unlinkedFiles.Add(file.Id);
                            payload.AdditionalFields.Add(new CredentialField { Label = attachment.Name, Type = CredentialFieldType.File, File = file });
                        }
                    }
                }
                catch (Exception exception) when (CanContinue(exception))
                {
                    Issues.Add(new ImportIssue(path, $"Attachment '{attachment.Name}' failed; verify storage before retrying."));
                }
                Advance("Importing attachments");
            }

            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                var saved = await client.CreateCredentialAsync(request.VaultId, directoryId, CredentialType.Login, payload, request.VaultKey);
                if (saved == null)
                {
                    _skippedEntries++;
                    Issues.Add(new ImportIssue(path, "Entry save was not confirmed. Check the vault before retrying; uploaded files may remain."));
                }
                else
                {
                    _hasWrites = true;
                    foreach (var fileId in uploaded) _unlinkedFiles.Remove(fileId);
                    if (Issues.Count == issueCount) _entries++;
                    else _partialEntries++;
                }
            }
            catch (Exception exception) when (CanContinue(exception))
            {
                _skippedEntries++;
                Issues.Add(new ImportIssue(path, "Entry save failed. Check the vault before retrying; uploaded files may remain."));
                if (exception is HttpRequestException { StatusCode: HttpStatusCode.BadRequest })
                    await CleanupAsync(uploaded, path);
            }
            Advance("Importing entries");
        }

        private async Task CleanupAsync(IEnumerable<Guid> files, string path)
        {
            foreach (var fileId in files)
            {
                try
                {
                    if (await client.DeleteFileAsync(fileId)) { _files--; _unlinkedFiles.Remove(fileId); }
                    else Issues.Add(new ImportIssue(path, $"Could not remove unlinked file {fileId}."));
                }
                catch (Exception exception) when (CanContinue(exception))
                {
                    Issues.Add(new ImportIssue(path, $"Could not remove unlinked file {fileId}."));
                }
            }
        }

        private void Advance(string phase, int count = 1)
        {
            _completed += count;
            progress?.Invoke(new ImportProgress(_completed, _total, phase));
        }

        private static bool CanContinue(Exception exception) => exception is not OperationCanceledException &&
            exception is not HttpRequestException { StatusCode: HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden };
    }
}