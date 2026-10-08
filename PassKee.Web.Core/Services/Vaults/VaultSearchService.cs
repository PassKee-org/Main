using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using PassKee.Api.Shared.Models.Vaults.Payloads;
using PassKee.Web.Models.Vaults;

namespace PassKee.Web.Core.Services.Vaults;

public class VaultSearchService : IVaultSearchService
{
    public Dictionary<Guid, string> BuildDirectoryPaths(IEnumerable<DecryptedDirectory> directories)
    {
        var dirList = directories.ToList();
        var dirMap = dirList.ToDictionary(d => d.Id, d => d);
        var result = new Dictionary<Guid, string>();

        string ResolvePath(Guid id, HashSet<Guid> visited)
        {
            if (result.TryGetValue(id, out var cached))
            {
                return cached;
            }

            if (!dirMap.TryGetValue(id, out var dir))
            {
                return string.Empty;
            }

            if (!visited.Add(id))
            {
                return dir.Name;
            }

            if (dir.ParentDirectoryId.HasValue && dirMap.ContainsKey(dir.ParentDirectoryId.Value))
            {
                var parentPath = ResolvePath(dir.ParentDirectoryId.Value, visited);
                var fullPath = string.IsNullOrWhiteSpace(parentPath) ? dir.Name : $"{parentPath} / {dir.Name}";
                result[id] = fullPath;
                return fullPath;
            }

            result[id] = dir.Name;
            return dir.Name;
        }

        foreach (var dir in dirList)
        {
            ResolvePath(dir.Id, new HashSet<Guid>());
        }

        return result;
    }

    public List<DecryptedDirectory> FilterDirectories(
        IEnumerable<DecryptedDirectory> directories,
        string query,
        IReadOnlyDictionary<Guid, string> directoryPaths,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return [];
        }

        var tokens = query.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (tokens.Length == 0)
        {
            return [];
        }

        var results = new List<DecryptedDirectory>();
        const StringComparison comparison = StringComparison.OrdinalIgnoreCase;

        foreach (var dir in directories)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            var path = directoryPaths.TryGetValue(dir.Id, out var p) ? p : dir.Name;
            var matchesAll = true;

            for (var i = 0; i < tokens.Length; i++)
            {
                var token = tokens[i];
                if (!dir.Name.Contains(token, comparison) && !path.Contains(token, comparison))
                {
                    matchesAll = false;
                    break;
                }
            }

            if (matchesAll)
            {
                results.Add(dir);
            }
        }

        return results;
    }

    public List<DecryptedCredential> Filter(
        IEnumerable<DecryptedCredential> credentials,
        string query,
        IReadOnlyDictionary<Guid, string> tagNames,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return credentials.ToList();
        }

        var tokens = query.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (tokens.Length == 0)
        {
            return credentials.ToList();
        }

        var results = new List<DecryptedCredential>();

        foreach (var cred in credentials)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            if (MatchesAllTokens(cred, tokens, tagNames))
            {
                results.Add(cred);
            }
        }

        return results;
    }

    public List<DecryptedCredential> Filter(
        IEnumerable<DecryptedCredential> credentials,
        string query,
        IReadOnlyDictionary<Guid, string> directoryPaths,
        IReadOnlyDictionary<Guid, string> tagNames,
        CancellationToken cancellationToken = default)
    {
        return Filter(credentials, query, tagNames, cancellationToken);
    }

    private static bool MatchesAllTokens(
        DecryptedCredential cred,
        string[] tokens,
        IReadOnlyDictionary<Guid, string> tagNames)
    {
        for (var i = 0; i < tokens.Length; i++)
        {
            var token = tokens[i];
            if (!MatchesToken(cred, token, tagNames))
            {
                return false;
            }
        }

        return true;
    }

    private static bool MatchesToken(
        DecryptedCredential cred,
        string token,
        IReadOnlyDictionary<Guid, string> tagNames)
    {
        const StringComparison comparison = StringComparison.OrdinalIgnoreCase;

        if (cred.Payload != null)
        {
            if (!string.IsNullOrEmpty(cred.Payload.Title) && cred.Payload.Title.Contains(token, comparison))
            {
                return true;
            }

            if (!string.IsNullOrWhiteSpace(cred.Payload.Notes) && cred.Payload.Notes.Contains(token, comparison))
            {
                return true;
            }

            if (cred.Payload is LoginCredentialPayload login)
            {
                if (!string.IsNullOrWhiteSpace(login.Username) && login.Username.Contains(token, comparison))
                {
                    return true;
                }

                if (!string.IsNullOrWhiteSpace(login.Website) && login.Website.Contains(token, comparison))
                {
                    return true;
                }
            }
            else if (cred.Payload is PasswordCredentialPayload pass)
            {
                if (!string.IsNullOrWhiteSpace(pass.Username) && pass.Username.Contains(token, comparison))
                {
                    return true;
                }
            }
            else if (cred.Payload is CardCredentialPayload card)
            {
                if (!string.IsNullOrWhiteSpace(card.CardholderName) && card.CardholderName.Contains(token, comparison))
                {
                    return true;
                }

                if (!string.IsNullOrWhiteSpace(card.CardNumber))
                {
                    var trimmedCard = card.CardNumber.Trim();
                    if (trimmedCard.Length >= 4 && trimmedCard[^4..].Contains(token, comparison))
                    {
                        return true;
                    }
                }
            }
            else if (cred.Payload is FileCredentialPayload filePayload)
            {
                if (!string.IsNullOrWhiteSpace(filePayload.File?.FileName) && filePayload.File.FileName.Contains(token, comparison))
                {
                    return true;
                }
            }
            else if (cred.Payload is SshKeyCredentialPayload sshKey)
            {
                if (!string.IsNullOrWhiteSpace(sshKey.PrivateKeyFile?.FileName) && sshKey.PrivateKeyFile.FileName.Contains(token, comparison))
                {
                    return true;
                }
            }
            else if (cred.Payload is ServerCredentialPayload server)
            {
                if (!string.IsNullOrWhiteSpace(server.Username) && server.Username.Contains(token, comparison))
                {
                    return true;
                }

                if (!string.IsNullOrWhiteSpace(server.Url) && server.Url.Contains(token, comparison))
                {
                    return true;
                }
            }

            if (cred.Payload.TagIds is { Count: > 0 })
            {
                foreach (var tagId in cred.Payload.TagIds)
                {
                    if (tagNames.TryGetValue(tagId, out var tagName) && tagName.Contains(token, comparison))
                    {
                        return true;
                    }
                }
            }

            if (cred.Payload.AdditionalFields is { Count: > 0 })
            {
                foreach (var field in cred.Payload.AdditionalFields)
                {
                    if (!string.IsNullOrWhiteSpace(field.Label) && field.Label.Contains(token, comparison))
                    {
                        return true;
                    }

                    if (!string.IsNullOrWhiteSpace(field.Value) && field.Value.Contains(token, comparison))
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }
}
