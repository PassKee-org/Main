using PassKee.Web.Core.Ui.Shared.Components.Storage;
using Xunit;

namespace PassKee.Tests.Unit.Web.Core.Ui;

public class FileIconHelperTests
{
    [Theory]
    [InlineData("report.pdf", "fa-solid fa-file-pdf")]
    [InlineData("REPORT.PDF", "fa-solid fa-file-pdf")]
    [InlineData("photo.final.JPG", "fa-solid fa-file-image")]
    [InlineData("archive.tar.gz", "fa-solid fa-file-zipper")]
    [InlineData("notes.txt", "fa-solid fa-file-lines")]
    [InlineData("sheet.xlsx", "fa-solid fa-file-excel")]
    [InlineData("unknown.xyz", "fa-solid fa-file")]
    [InlineData("no_extension", "fa-solid fa-file")]
    [InlineData("", "fa-solid fa-file")]
    [InlineData(null, "fa-solid fa-file")]
    public void GetIconClass_Picks_Icon_By_Extension(string? fileName, string expected)
    {
        Assert.Equal(expected, FileIconHelper.GetIconClass(fileName));
    }
}
