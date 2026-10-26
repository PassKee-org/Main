using Microsoft.AspNetCore.Components;
using PassKee.Web.Server.Models;

namespace PassKee.Web.Server.Components.Pages;

public partial class SecurityLandingPage
{
    [CascadingParameter]
    private HttpContext? HttpContext { get; set; }

    private string BaseUrl =>
        HttpContext?.Request.Host.HasValue == true
            ? $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.Value}"
            : "https://passkee.org";

    private string CanonicalUrl => $"{BaseUrl}/security";

    private string JsonLd => LandingContentData.BuildSecurityJsonLd(BaseUrl);
}

