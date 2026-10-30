using PassKee.Business.Common.Utils;
using Xunit;

namespace PassKee.Tests.Unit.Business.Common.Utils;

public class PaginationUtilsTest
{
    [Theory]
    [InlineData(0, 10, 1)]
    [InlineData(10, 10, 2)]
    [InlineData(25, 10, 3)]
    public void CalculatePage_ShouldCalculateCorrectPageNumber(int skip, int pageSize, int expectedPage)
    {
        var page = PaginationUtils.CalculatePage(skip, pageSize);
        Assert.Equal(expectedPage, page);
    }

    [Theory]
    [InlineData(1, 10, 0)]
    [InlineData(2, 10, 10)]
    [InlineData(3, 15, 30)]
    public void CalculateOffset_ShouldCalculateCorrectOffset(int page, int pageSize, int expectedOffset)
    {
        var offset = PaginationUtils.CalculateOffset(page, pageSize);
        Assert.Equal(expectedOffset, offset);
    }

    [Theory]
    [InlineData(100, 10, 10)]
    [InlineData(95, 10, 9)]
    [InlineData(0, 10, 0)]
    public void CalculateTotalPages_ShouldCalculateCorrectTotalPages(int total, int pageSize, int expectedTotalPages)
    {
        var totalPages = PaginationUtils.CalculateTotalPages(total, pageSize);
        Assert.Equal(expectedTotalPages, totalPages);
    }
}
