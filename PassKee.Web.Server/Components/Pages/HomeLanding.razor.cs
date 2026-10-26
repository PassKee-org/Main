using Microsoft.AspNetCore.Components;
using PassKee.Web.Server.Models;

namespace PassKee.Web.Server.Components.Pages;

public partial class HomeLanding
{
    [CascadingParameter]
    private HttpContext? HttpContext { get; set; }

    private string BaseUrl =>
        HttpContext?.Request.Host.HasValue is true
            ? $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.Value}"
            : "https://passkee.org";

    private string CanonicalUrl => $"{BaseUrl}/";

    private string JsonLd => LandingContentData.BuildHomeJsonLd(BaseUrl);
}

