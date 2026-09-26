using System.Linq.Expressions;
using Microsoft.AspNetCore.Components.QuickGrid;

namespace PassKee.Web.Core.Ui.Shared.Components.Table;

public static class AppGridSort
{
    public static GridSort<TGridItem> ByAscending<TGridItem, TProp>(Expression<Func<TGridItem, TProp>> expression)
        => GridSort<TGridItem>.ByAscending(expression);

    public static GridSort<TGridItem> ByDescending<TGridItem, TProp>(Expression<Func<TGridItem, TProp>> expression)
        => GridSort<TGridItem>.ByDescending(expression);
}
