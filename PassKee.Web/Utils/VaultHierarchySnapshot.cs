using System;
using System.Collections.Generic;
using System.Linq;
using PassKee.Web.Constants;
using PassKee.Web.Core.Services.Vaults;
using PassKee.Web.Models.Vaults;

namespace PassKee.Web.Utils;

public sealed class VaultHierarchySnapshot
{
    public IReadOnlyDictionary<Guid, DecryptedDirectory> DirectoryMap { get; }
    public IReadOnlyDictionary<Guid, List<DecryptedDirectory>> ChildrenMap { get; }
    public IReadOnlyList<DecryptedDirectory> RootDirectories { get; }
    public IReadOnlyDictionary<Guid, int> DirectDirectoryCounts { get; }
    public IReadOnlyDictionary<Guid, int> TotalDirectoryCounts { get; }
    public int RootCredentialCount { get; }
    public IReadOnlyDictionary<Guid, string> DirectoryPaths { get; }
    public IReadOnlyDictionary<Guid, string> TagNames { get; }
    public IReadOnlyDictionary<Guid, List<DecryptedCredential>> CredentialsByDirectory { get; }
    public IReadOnlyList<DecryptedCredential> RootCredentials { get; }

    public VaultHierarchySnapshot(
        IReadOnlyList<DecryptedDirectory> directories,
        IReadOnlyList<DecryptedCredential> credentials,
        IReadOnlyList<DecryptedTag> tags,
        IVaultSearchService searchService)
    {
        var dirMap = new Dictionary<Guid, DecryptedDirectory>(directories.Count);
        var children = new Dictionary<Guid, List<DecryptedDirectory>>();
        var roots = new List<DecryptedDirectory>();

        foreach (var dir in directories)
        {
            dirMap[dir.Id] = dir;
        }

        foreach (var dir in directories)
        {
            if (dir.ParentDirectoryId.HasValue && dirMap.ContainsKey(dir.ParentDirectoryId.Value))
            {
                if (!children.TryGetValue(dir.ParentDirectoryId.Value, out var list))
                {
                    list = [];
                    children[dir.ParentDirectoryId.Value] = list;
                }
                list.Add(dir);
            }
            else
            {
                roots.Add(dir);
            }
        }

        var byDir = new Dictionary<Guid, List<DecryptedCredential>>();
        var rootCreds = new List<DecryptedCredential>();
        var directCounts = new Dictionary<Guid, int>();
        var rootCount = 0;

        foreach (var cred in credentials.OrderBy(c => c.Order))
        {
            if (cred.DirectoryId.HasValue)
            {
                var dirId = cred.DirectoryId.Value;
                if (!byDir.TryGetValue(dirId, out var list))
                {
                    list = [];
                    byDir[dirId] = list;
                }
                list.Add(cred);
                directCounts[dirId] = directCounts.GetValueOrDefault(dirId) + 1;
            }
            else
            {
                rootCreds.Add(cred);
                rootCount++;
            }
        }

        var totalCounts = new Dictionary<Guid, int>(directories.Count);
        int ComputeTotalCount(Guid dirId, HashSet<Guid> visited)
        {
            if (totalCounts.TryGetValue(dirId, out var cached))
            {
                return cached;
            }

            if (!visited.Add(dirId))
            {
                return 0;
            }

            var total = directCounts.GetValueOrDefault(dirId);
            if (children.TryGetValue(dirId, out var childDirs))
            {
                foreach (var child in childDirs)
                {
                    total += ComputeTotalCount(child.Id, visited);
                }
            }

            totalCounts[dirId] = total;
            return total;
        }

        foreach (var dir in directories)
        {
            ComputeTotalCount(dir.Id, []);
        }

        DirectoryMap = dirMap;
        ChildrenMap = children;
        RootDirectories = roots;
        DirectDirectoryCounts = directCounts;
        TotalDirectoryCounts = totalCounts;
        RootCredentialCount = rootCount;
        DirectoryPaths = searchService.BuildDirectoryPaths(directories);
        TagNames = tags.ToDictionary(t => t.Id, t => t.Name);
        CredentialsByDirectory = byDir;
        RootCredentials = rootCreds;
    }

    public IReadOnlyList<DecryptedDirectory> GetSubdirectories(Guid? parentDirectoryId)
    {
        if (!parentDirectoryId.HasValue)
        {
            return RootDirectories;
        }

        return ChildrenMap.TryGetValue(parentDirectoryId.Value, out var list) ? list : [];
    }

    public IReadOnlyList<DecryptedCredential> GetDirectoryCredentials(Guid? directoryId)
    {
        if (!directoryId.HasValue)
        {
            return RootCredentials;
        }

        return CredentialsByDirectory.TryGetValue(directoryId.Value, out var list) ? list : [];
    }

    public int GetDirectItemCount(Guid? directoryId)
    {
        if (!directoryId.HasValue)
        {
            return RootCredentialCount;
        }

        return DirectDirectoryCounts.GetValueOrDefault(directoryId.Value);
    }

    public int GetTotalItemCount(Guid directoryId) =>
        TotalDirectoryCounts.GetValueOrDefault(directoryId);

    public int GetSubdirectoryCount(Guid? directoryId) =>
        GetSubdirectories(directoryId).Count;

    public string GetDirectoryPath(Guid? directoryId)
    {
        if (directoryId.HasValue && DirectoryPaths.TryGetValue(directoryId.Value, out var path))
        {
            return path;
        }
        return string.Empty;
    }

    public Guid? GetParentDirectoryId(Guid? directoryId)
    {
        if (directoryId.HasValue && DirectoryMap.TryGetValue(directoryId.Value, out var dir))
        {
            return dir.ParentDirectoryId;
        }
        return null;
    }

    public HashSet<Guid> GetExpandedAncestorIds(Guid? selectedDirectoryId)
    {
        var ancestors = new HashSet<Guid>();
        var currentId = selectedDirectoryId;
        while (currentId.HasValue && DirectoryMap.TryGetValue(currentId.Value, out var dir))
        {
            if (dir.ParentDirectoryId.HasValue)
            {
                ancestors.Add(dir.ParentDirectoryId.Value);
            }
            currentId = dir.ParentDirectoryId;
        }
        return ancestors;
    }

    public List<DecryptedDirectory> GetBreadcrumbChain(Guid? selectedDirectoryId)
    {
        var chain = new List<DecryptedDirectory>();
        var visited = new HashSet<Guid>();
        var currentId = selectedDirectoryId;
        while (currentId.HasValue && visited.Add(currentId.Value) && DirectoryMap.TryGetValue(currentId.Value, out var dir))
        {
            chain.Add(dir);
            currentId = dir.ParentDirectoryId;
        }
        chain.Reverse();
        return chain;
    }

    public static IReadOnlyList<DecryptedCredential> ApplySort(
        IEnumerable<DecryptedCredential> credentials,
        CredentialSortOrder sortOrder)
    {
        return sortOrder switch
        {
            CredentialSortOrder.TitleAsc => credentials
                .OrderBy(c => c.Payload?.Title ?? string.Empty, StringComparer.OrdinalIgnoreCase)
                .ThenBy(c => c.Order)
                .ToList(),
            CredentialSortOrder.TitleDesc => credentials
                .OrderByDescending(c => c.Payload?.Title ?? string.Empty, StringComparer.OrdinalIgnoreCase)
                .ThenBy(c => c.Order)
                .ToList(),
            _ => credentials as IReadOnlyList<DecryptedCredential> ?? credentials.ToList()
        };
    }
}
