using Microsoft.AspNetCore.Components;
using PassKee.Web.Server.Models;

namespace PassKee.Web.Server.Components.Pages.Parts;

public partial class HelpFaqListBlock
{
    [Parameter]
    public string? SearchQuery { get; set; }

    [Parameter]
    public string ActiveCategory { get; set; } = "all";

    private static IReadOnlyList<FaqItemModel> AllFaqItems => LandingContentData.FaqItems;

    private bool MatchesServerFilter(FaqItemModel item)
    {
        if (!string.Equals(ActiveCategory, "all", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(item.CategoryId, ActiveCategory, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(SearchQuery))
        {
            var q = SearchQuery.Trim();
            return item.Question.Contains(q, StringComparison.OrdinalIgnoreCase)
                || item.Answer.Contains(q, StringComparison.OrdinalIgnoreCase)
                || item.CategoryLabel.Contains(q, StringComparison.OrdinalIgnoreCase)
                || item.Keywords.Any(k => k.Contains(q, StringComparison.OrdinalIgnoreCase));
        }

        return true;
    }

    private static string BuildSearchBlob(FaqItemModel item)
        => $"{item.Question} {item.Answer} {item.CategoryLabel} {string.Join(' ', item.Keywords)}".ToLowerInvariant();
}

