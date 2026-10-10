using System;
using Microsoft.AspNetCore.Components;

namespace PassKee.Tests.Unit.Web.Mocks;

public class TestNavigationManager : NavigationManager
{
    public string? NavigatedToUri { get; private set; }

    public TestNavigationManager(string baseUri = "http://localhost/", string initialUri = "http://localhost/")
    {
        Initialize(baseUri, initialUri);
    }

    protected override void NavigateToCore(string uri, NavigationOptions options)
    {
        NavigatedToUri = uri;
    }
}
