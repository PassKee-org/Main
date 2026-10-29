using System;
using System.Collections.Generic;
using PassKee.Api.Shared.Models.Vaults.Payloads;
using PassKee.Business.Common.Constants;
using PassKee.Web.Constants;
using PassKee.Web.Core.Services.Vaults;
using PassKee.Web.Models.Vaults;
using PassKee.Web.Utils;
using Xunit;

namespace PassKee.Tests.Unit.Web.Utils;

public class VaultHierarchySnapshotTests
{
    private readonly VaultSearchService _searchService = new();
    private readonly Guid _vaultId = Guid.NewGuid();

    [Fact]
    public void Snapshot_ComputesDirectAndRecursiveCountsAndBreadcrumbs()
    {
        var devId = Guid.NewGuid();
        var projectsId = Guid.NewGuid();
        var mainframeId = Guid.NewGuid();

        var directories = new List<DecryptedDirectory>
        {
            new(devId, _vaultId, null, "Dev"),
            new(projectsId, _vaultId, devId, "Projects"),
            new(mainframeId, _vaultId, projectsId, "MainFrame")
        };

        var credentials = new List<DecryptedCredential>
        {
            CreateCredential("Root Item", null, order: 0),
            CreateCredential("Dev Item 1", devId, order: 0),
            CreateCredential("Projects Item 1", projectsId, order: 0),
            CreateCredential("MainFrame Item 1", mainframeId, order: 0),
            CreateCredential("MainFrame Item 2", mainframeId, order: 1)
        };

        var snapshot = new VaultHierarchySnapshot(directories, credentials, [], _searchService);

        Assert.Equal(1, snapshot.RootCredentialCount);
        Assert.Single(snapshot.RootDirectories);
        Assert.Equal(devId, snapshot.RootDirectories[0].Id);

        // Direct counts
        Assert.Equal(1, snapshot.GetDirectItemCount(devId));
        Assert.Equal(1, snapshot.GetDirectItemCount(projectsId));
        Assert.Equal(2, snapshot.GetDirectItemCount(mainframeId));

        // Recursive total counts (for mobile drill-down folder badges)
        Assert.Equal(4, snapshot.GetTotalItemCount(devId));
        Assert.Equal(3, snapshot.GetTotalItemCount(projectsId));
        Assert.Equal(2, snapshot.GetTotalItemCount(mainframeId));

        // Breadcrumbs & Parent navigation
        var chain = snapshot.GetBreadcrumbChain(mainframeId);
        Assert.Equal(3, chain.Count);
        Assert.Equal("Dev", chain[0].Name);
        Assert.Equal("Projects", chain[1].Name);
        Assert.Equal("MainFrame", chain[2].Name);

        Assert.Equal(projectsId, snapshot.GetParentDirectoryId(mainframeId));
        Assert.Equal(devId, snapshot.GetParentDirectoryId(projectsId));
        Assert.Null(snapshot.GetParentDirectoryId(devId));

        // Expanded ancestors
        var expanded = snapshot.GetExpandedAncestorIds(mainframeId);
        Assert.Contains(devId, expanded);
        Assert.Contains(projectsId, expanded);
        Assert.DoesNotContain(mainframeId, expanded);
    }

    [Fact]
    public void ApplySort_SortsByCustomTitleAscAndTitleDesc()
    {
        var credentials = new List<DecryptedCredential>
        {
            CreateCredential("Bravo", null, order: 0),
            CreateCredential("Alpha", null, order: 1),
            CreateCredential("Charlie", null, order: 2)
        };

        var custom = VaultHierarchySnapshot.ApplySort(credentials, CredentialSortOrder.Custom);
        Assert.Equal(["Bravo", "Alpha", "Charlie"], [custom[0].Payload.Title, custom[1].Payload.Title, custom[2].Payload.Title]);

        var asc = VaultHierarchySnapshot.ApplySort(credentials, CredentialSortOrder.TitleAsc);
        Assert.Equal(["Alpha", "Bravo", "Charlie"], [asc[0].Payload.Title, asc[1].Payload.Title, asc[2].Payload.Title]);

        var desc = VaultHierarchySnapshot.ApplySort(credentials, CredentialSortOrder.TitleDesc);
        Assert.Equal(["Charlie", "Bravo", "Alpha"], [desc[0].Payload.Title, desc[1].Payload.Title, desc[2].Payload.Title]);
    }

    private DecryptedCredential CreateCredential(string title, Guid? dirId, int order) =>
        new(
            Guid.NewGuid(),
            _vaultId,
            dirId,
            CredentialType.Login,
            new LoginCredentialPayload
            {
                Title = title,
                Username = "user",
                Password = "secret"
            },
            Order: order);
}
