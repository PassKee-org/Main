using Microsoft.AspNetCore.Components;
using PassKee.Web.Server.Models;

namespace PassKee.Web.Server.Components.Pages.Parts;

public partial class HelpSearchHeaderBlock
{
    [Parameter]
    public string? SearchQuery { get; set; }

    [Parameter]
    public string ActiveCategory { get; set; } = "all";

    private static IReadOnlyList<FaqCategoryModel> Categories => LandingContentData.FaqCategories;

    private string GetCategoryPillClass(string categoryId)
    {
        var isActive = string.Equals(ActiveCategory, categoryId, StringComparison.OrdinalIgnoreCase);
        return isActive
            ? "inline-flex items-center gap-2 rounded-xl border border-blue-600 bg-blue-600 px-3.5 py-2 text-xs sm:text-sm font-semibold text-white shadow-sm transition-colors cursor-pointer"
            : "inline-flex items-center gap-2 rounded-xl border border-slate-200 bg-white px-3.5 py-2 text-xs sm:text-sm font-medium text-slate-700 hover:border-blue-300 hover:bg-blue-50/50 transition-colors cursor-pointer";
    }
}

