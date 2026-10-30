using System;
using System.Collections.Generic;
using PassKee.Api.Shared.Models.Vaults.Payloads;
using PassKee.Business.Common.Constants;
using PassKee.Web.Core.Services.Vaults;
using PassKee.Web.Models.Vaults;
using Xunit;

namespace PassKee.Tests.Unit.Web.Core.Services;

public class VaultSearchServiceTests
{
    private readonly VaultSearchService _searchService = new();

    [Fact]
    public void BuildDirectoryPaths_ShouldReturnCorrectHierarchicalPaths()
    {
        var rootId = Guid.NewGuid();
        var subId = Guid.NewGuid();
        var childId = Guid.NewGuid();

        var directories = new List<DecryptedDirectory>
        {
            new(rootId, Guid.NewGuid(), null, "Work"),
            new(subId, Guid.NewGuid(), rootId, "Projects"),
            new(childId, Guid.NewGuid(), subId, "SecretApp")
        };

        var paths = _searchService.BuildDirectoryPaths(directories);

        Assert.Equal("Work", paths[rootId]);
        Assert.Equal("Work / Projects", paths[subId]);
        Assert.Equal("Work / Projects / SecretApp", paths[childId]);
    }

    [Fact]
    public void Filter_EmptyQuery_ShouldReturnAllCredentials()
    {
        var creds = new List<DecryptedCredential>
        {
            CreateCredential("Google", "john@gmail.com"),
            CreateCredential("GitHub", "johndoe")
        };

        var result = _searchService.Filter(creds, "", new Dictionary<Guid, string>());

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void Filter_ByTitle_ShouldMatchCaseInsensitively()
    {
        var creds = new List<DecryptedCredential>
        {
            CreateCredential("GitHub Account", "dev"),
            CreateCredential("Google Workspace", "dev"),
            CreateCredential("GitLab", "dev")
        };

        var result = _searchService.Filter(creds, "git", new Dictionary<Guid, string>());

        Assert.Equal(2, result.Count);
        Assert.Contains(result, c => c.Payload.Title == "GitHub Account");
        Assert.Contains(result, c => c.Payload.Title == "GitLab");
    }

    [Fact]
    public void FilterDirectories_ShouldMatchByNameOrPath()
    {
        var rootId = Guid.NewGuid();
        var childId = Guid.NewGuid();
        var directories = new List<DecryptedDirectory>
        {
            new(rootId, Guid.NewGuid(), null, "Dev"),
            new(childId, Guid.NewGuid(), rootId, "People")
        };

        var paths = new Dictionary<Guid, string>
        {
            [rootId] = "Dev",
            [childId] = "Dev / People"
        };

        var result = _searchService.FilterDirectories(directories, "people", paths);

        Assert.Single(result);
        Assert.Equal("People", result[0].Name);
    }

    [Fact]
    public void FilterCredentials_ShouldNotMatchSolelyByDirectoryName()
    {
        var dirId = Guid.NewGuid();
        var creds = new List<DecryptedCredential>
        {
            CreateCredential("Database Password", "admin", dirId: dirId),
            CreateCredential("Email Account", "user")
        };

        var result = _searchService.Filter(creds, "production", new Dictionary<Guid, string>());

        // Neither credential contains "production" in its own fields
        Assert.Empty(result);
    }

    [Fact]
    public void Filter_MultipleTokens_MustMatchAllTokens()
    {
        var creds = new List<DecryptedCredential>
        {
            CreateCredential("Google Cloud", "admin@company.com"),
            CreateCredential("Google Personal", "user@gmail.com"),
            CreateCredential("AWS Cloud", "admin@company.com")
        };

        var result = _searchService.Filter(creds, "google admin", new Dictionary<Guid, string>());

        Assert.Single(result);
        Assert.Equal("Google Cloud", result[0].Payload.Title);
    }

    [Fact]
    public void Filter_ByTag_ShouldMatch()
    {
        var tagId = Guid.NewGuid();
        var cred = CreateCredential("Secret Key", "user");
        cred.Payload.TagIds.Add(tagId);

        var creds = new List<DecryptedCredential> { cred };
        var tagNames = new Dictionary<Guid, string> { [tagId] = "ProductionKeys" };

        var result = _searchService.Filter(creds, "ProductionKeys", tagNames);

        Assert.Single(result);
    }

    private static DecryptedCredential CreateCredential(string title, string username, Guid? dirId = null)
    {
        return new DecryptedCredential(
            Guid.NewGuid(),
            Guid.NewGuid(),
            dirId,
            CredentialType.Login,
            new LoginCredentialPayload
            {
                Title = title,
                Username = username,
                Password = "pwd"
            }
        );
    }
}
