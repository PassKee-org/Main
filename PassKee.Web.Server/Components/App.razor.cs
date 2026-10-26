using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace PassKee.Web.Server.Components;

public partial class App
{
    private static readonly HashSet<string> StaticServerPaths = new(StringComparer.OrdinalIgnoreCase)
    {
        "/",
        "/security",
        "/help",
        "/faq",
        "/not-found",
        "/Error"
    };

    [CascadingParameter]
    private HttpContext HttpContext { get; set; } = default!;

    private bool IsStaticServerPage
    {
        get
        {
            var path = HttpContext.Request.Path.HasValue ? HttpContext.Request.Path.Value! : "/";
            var normalizedPath = path.Length > 1 ? path.TrimEnd('/') : path;
            return StaticServerPaths.Contains(normalizedPath);
        }
    }

    private IComponentRenderMode? PageRenderMode
        => IsStaticServerPage
            ? null
            : new InteractiveWebAssemblyRenderMode(prerender: false);
}

