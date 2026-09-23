using PassKee.Business.Common.Constants;

namespace PassKee.Business.Common.Utils;

public static class PaginationUtils
{
    public static int DefaultPageSize => GlobalConstants.ListPageSize;
    
    public static int CalculatePage(int skip, int pageSize = GlobalConstants.ListPageSize)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(skip);
        ArgumentOutOfRangeException.ThrowIfNegative(pageSize);
        
        var page = (int) Math.Ceiling((decimal) (skip / pageSize));
        return page == 0 ? 1 : page + 1;
    }
    
    public static int CalculateOffset(int page, int pageSize = GlobalConstants.ListPageSize)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(page);
        ArgumentOutOfRangeException.ThrowIfNegative(pageSize);
        
        page = page - 1;
        return (int)((page <= 0 ? 0 : page) * pageSize);
    }
    
    public static int CalculateTotalPages(int total, int pageSize = GlobalConstants.ListPageSize)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(total);
        ArgumentOutOfRangeException.ThrowIfNegative(pageSize);
        
        return (int)Math.Round((decimal)(total / pageSize));
    }
}
