using PassKee.Web.Server.Models;

namespace PassKee.Web.Server.Components.Pages.Parts;

public partial class HomeFaqPreviewBlock
{
    private static IReadOnlyList<FaqItemModel> TopFaqItems { get; } =
        LandingContentData.FaqItems.Where(f => f.ShowOnHome).ToList();
}

