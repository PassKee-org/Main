using Microsoft.AspNetCore.Components;

namespace PassKee.Web.Server.Components.Layout;

public partial class StaticLandingLayout : LayoutComponentBase
{
    [CascadingParameter]
    private HttpContext? HttpContext { get; set; }

    private string NormalizedPath
    {
        get
        {
            var rawPath = HttpContext?.Request.Path.HasValue is true
                ? HttpContext.Request.Path.Value!
                : "/";
            return rawPath.Length > 1 ? rawPath.TrimEnd('/') : rawPath;
        }
    }

    private bool IsActive(string targetPath)
    {
        if (string.Equals(targetPath, "/help", StringComparison.OrdinalIgnoreCase) &&
            string.Equals(NormalizedPath, "/faq", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return string.Equals(NormalizedPath, targetPath, StringComparison.OrdinalIgnoreCase);
    }

    private string GetNavLinkClass(string targetPath)
        => IsActive(targetPath)
            ? "inline-flex items-center gap-1.5 rounded-lg bg-blue-50 px-3 py-1.5 text-sm font-semibold text-blue-700 transition-colors"
            : "inline-flex items-center gap-1.5 rounded-lg px-3 py-1.5 text-sm font-medium text-slate-600 hover:bg-slate-100 hover:text-slate-900 transition-colors";

    private string? GetAriaCurrent(string targetPath)
        => IsActive(targetPath) ? "page" : null;
}

