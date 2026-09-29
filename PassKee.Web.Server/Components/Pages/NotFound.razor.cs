using Microsoft.AspNetCore.Components;

namespace PassKee.Web.Server.Components.Pages;

public partial class NotFound
{
    [CascadingParameter]
    private HttpContext? HttpContext { get; set; }

    protected override void OnInitialized()
    {
        if (HttpContext is not null)
        {
            HttpContext.Response.StatusCode = StatusCodes.Status404NotFound;
        }
    }
}