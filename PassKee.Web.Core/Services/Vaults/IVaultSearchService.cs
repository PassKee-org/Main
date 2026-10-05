using System;
using System.Collections.Generic;
using System.Threading;
using PassKee.Web.Models.Vaults;

namespace PassKee.Web.Core.Services.Vaults;

public interface IVaultSearchService
{
    Dictionary<Guid, string> BuildDirectoryPaths(IEnumerable<DecryptedDirectory> directories);

    List<DecryptedDirectory> FilterDirectories(
        IEnumerable<DecryptedDirectory> directories,
        string query,
        IReadOnlyDictionary<Guid, string> directoryPaths,
        CancellationToken cancellationToken = default);

    List<DecryptedCredential> Filter(
        IEnumerable<DecryptedCredential> credentials,
        string query,
        IReadOnlyDictionary<Guid, string> tagNames,
        CancellationToken cancellationToken = default);

    List<DecryptedCredential> Filter(
        IEnumerable<DecryptedCredential> credentials,
        string query,
        IReadOnlyDictionary<Guid, string> directoryPaths,
        IReadOnlyDictionary<Guid, string> tagNames,
        CancellationToken cancellationToken = default);
}
