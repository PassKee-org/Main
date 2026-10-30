using System;
using System.Collections.Generic;
using PassKee.Api.Shared.Models.Vaults.Payloads;
using PassKee.Web.Core.Utils;
using Xunit;

namespace PassKee.Tests.Unit.Web.Core.Utils;

public class CredentialClonerTests
{
    [Fact]
    public void CloneWithCopyTitle_WhenTitleNotEmpty_AppendsCopy()
    {
        var original = new LoginCredentialPayload
        {
            Title = "Google Account",
            Username = "user@example.com",
            Password = "secret-password",
            Website = "https://google.com",
            TagIds = [Guid.NewGuid()]
        };

        var clone = CredentialCloner.CloneWithCopyTitle(original);

        Assert.IsType<LoginCredentialPayload>(clone);
        var loginClone = (LoginCredentialPayload)clone;
        Assert.Equal("Google Account Copy", loginClone.Title);
        Assert.Equal("user@example.com", loginClone.Username);
        Assert.Equal("secret-password", loginClone.Password);
        Assert.Equal("https://google.com", loginClone.Website);
        Assert.Equal(original.TagIds, loginClone.TagIds);
        Assert.NotSame(original, clone);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void CloneWithCopyTitle_WhenTitleIsEmptyOrWhitespace_SetsTitleToCopy(string title)
    {
        var original = new PasswordCredentialPayload
        {
            Title = title,
            Username = "admin",
            Password = "password123"
        };

        var clone = CredentialCloner.CloneWithCopyTitle(original);

        Assert.IsType<PasswordCredentialPayload>(clone);
        Assert.Equal("Copy", clone.Title);
    }

    [Fact]
    public void CloneWithCopyTitle_WhenSectionsAndAdditionalFieldsExist_DeepClonesThem()
    {
        var original = new SecureNoteCredentialPayload
        {
            Title = "Secret Note",
            Notes = "Confidential text",
            AdditionalFields =
            [
                new CredentialField { Label = "Pin", Value = "1234", Type = CredentialFieldType.Text }
            ],
            Sections =
            [
                new CredentialSection
                {
                    Title = "Extra",
                    Fields = [new CredentialField { Label = "Key", Value = "Val" }]
                }
            ]
        };

        var clone = (SecureNoteCredentialPayload)CredentialCloner.CloneWithCopyTitle(original);

        Assert.Equal("Secret Note Copy", clone.Title);
        Assert.Equal("Confidential text", clone.Notes);
        Assert.Single(clone.AdditionalFields);
        Assert.Equal("1234", clone.AdditionalFields[0].Value);
        Assert.NotSame(original.AdditionalFields, clone.AdditionalFields);
        Assert.Single(clone.Sections);
        Assert.NotSame(original.Sections, clone.Sections);
    }

    [Fact]
    public void CloneWithCopyTitle_WhenIconIsSet_PreservesIcon()
    {
        var original = new LoginCredentialPayload
        {
            Title = "My Bank",
            Icon = "🏦"
        };

        var clone = CredentialCloner.CloneWithCopyTitle(original);

        Assert.Equal("🏦", clone.Icon);
    }
}
