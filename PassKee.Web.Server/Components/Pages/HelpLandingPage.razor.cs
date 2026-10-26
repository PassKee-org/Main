using Microsoft.AspNetCore.Components;
using PassKee.Web.Server.Models;

namespace PassKee.Web.Server.Components.Pages;

public partial class HelpLandingPage
{
    [CascadingParameter]
    private HttpContext? HttpContext { get; set; }

    [SupplyParameterFromQuery(Name = "q")]
    public string? SearchQuery { get; set; }

    [SupplyParameterFromQuery(Name = "category")]
    public string? Category { get; set; }

    private string ActiveCategory =>
        string.IsNullOrWhiteSpace(Category) ? "all" : Category.Trim().ToLowerInvariant();

    private string BaseUrl =>
        HttpContext?.Request.Host.HasValue == true
            ? $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.Value}"
            : "https://passkee.org";

    private string CanonicalUrl => $"{BaseUrl}/help";

    private string JsonLd => LandingContentData.BuildHelpJsonLd(BaseUrl);
}

