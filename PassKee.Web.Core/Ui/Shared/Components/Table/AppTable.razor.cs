using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.QuickGrid;

namespace PassKee.Web.Core.Ui.Shared.Components.Table;

[CascadingTypeParameter(nameof(TGridItem))]
public partial class AppTable<TGridItem> : ComponentBase
{
    [Parameter]
    public IQueryable<TGridItem>? Items { get; set; }

    [Parameter]
    public GridItemsProvider<TGridItem>? ItemsProvider { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public bool IsLoading { get; set; }

    [Parameter]
    public bool Loading
    {
        get => IsLoading;
        set => IsLoading = value;
    }

    [Parameter]
    public bool IsHoverable { get; set; } = true;

    [Parameter]
    public bool Hoverable
    {
        get => IsHoverable;
        set => IsHoverable = value;
    }

    [Parameter]
    public bool IsStriped { get; set; } = true;

    [Parameter]
    public bool Striped
    {
        get => IsStriped;
        set => IsStriped = value;
    }

    [Parameter]
    public bool HasRowBorders { get; set; } = true;

    [Parameter]
    public bool WithRowBorders
    {
        get => HasRowBorders;
        set => HasRowBorders = value;
    }

    [Parameter]
    public bool HasColumnBorders { get; set; }

    [Parameter]
    public bool WithColumnBorders
    {
        get => HasColumnBorders;
        set => HasColumnBorders = value;
    }

    [Parameter]
    public bool HasTableBorder { get; set; } = true;

    [Parameter]
    public bool WithTableBorder
    {
        get => HasTableBorder;
        set => HasTableBorder = value;
    }

    [Parameter]
    public string? Theme { get; set; } = "tailwind";

    [Parameter]
    public string Class { get; set; } = string.Empty;

    [Parameter]
    public string? ContainerClass { get; set; }

    [Parameter]
    public Func<TGridItem, string>? RowClass { get; set; }

    [Parameter]
    public Func<TGridItem, string>? RowStyle { get; set; }

    [Parameter]
    public PaginationState? Pagination { get; set; }

    [Parameter]
    public bool Virtualize { get; set; }

    [Parameter]
    public float ItemSize { get; set; } = 50;

    [Parameter]
    public Func<TGridItem, object> ItemKey { get; set; } = x => x!;

    [Parameter]
    public string? EmptyText { get; set; }

    [Parameter]
    public string? LoadingText { get; set; }

    [Parameter]
    public RenderFragment? EmptyContent { get; set; }

    [Parameter]
    public RenderFragment? LoadingContent { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    protected bool IsEmpty
    {
        get
        {
            if (Items != null)
            {
                return !Items.Any();
            }

            return false;
        }
    }

    protected string ComputedContainerClass =>
        $"app-table-container w-full overflow-x-auto rounded-xl {(HasTableBorder ? "border border-slate-200 dark:border-slate-700" : "")} bg-white dark:bg-slate-800 {ContainerClass}".Trim();

    protected string ComputedGridClass
    {
        get
        {
            var classes = new List<string>
            {
                "w-full text-left text-sm border-collapse quickgrid-table theme-tailwind"
            };

            if (IsStriped)
            {
                classes.Add("table-striped");
            }

            if (IsHoverable)
            {
                classes.Add("table-hover");
            }

            if (HasRowBorders)
            {
                classes.Add("table-row-borders");
            }

            if (HasColumnBorders)
            {
                classes.Add("table-column-borders");
            }

            if (!string.IsNullOrWhiteSpace(Class))
            {
                classes.Add(Class);
            }

            return string.Join(" ", classes);
        }
    }

    protected string GetRowClass(TGridItem item)
    {
        var customClass = RowClass?.Invoke(item) ?? string.Empty;
        var hoverClass = IsHoverable ? "app-table-row-hover" : string.Empty;
        return $"{hoverClass} {customClass}".Trim();
    }

    protected string GetEmptyText()
    {
        if (!string.IsNullOrWhiteSpace(EmptyText))
        {
            return EmptyText;
        }

        return "No data";
    }

    protected string GetLoadingText()
    {
        if (!string.IsNullOrWhiteSpace(LoadingText))
        {
            return LoadingText;
        }

        return "Loading...";
    }
}
