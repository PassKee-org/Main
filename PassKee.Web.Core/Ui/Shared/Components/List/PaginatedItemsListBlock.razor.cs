using Microsoft.AspNetCore.Components;

namespace PassKee.Web.Core.Ui.Shared.Components.List;

public partial class PaginatedItemsListBlock<TItem> : ComponentBase
{
    [Parameter]
    public required IEnumerable<TItem> Items { get; set; }

    [Parameter]
    public required RenderFragment<TItem> ItemTemplate { get; set; }

    [Parameter]
    public RenderFragment? EmptyContent { get; set; }

    [Parameter]
    public bool IsLoading { get; set; }

    [Parameter]
    public bool IsHasMore { get; set; }

    [Parameter]
    public EventCallback OnLoadMore { get; set; }

    [Parameter]
    public string LoadMoreText { get; set; } = string.Empty;

    [Parameter]
    public string ItemsClass { get; set; } = "divide-y divide-slate-200 dark:divide-slate-700";
}
