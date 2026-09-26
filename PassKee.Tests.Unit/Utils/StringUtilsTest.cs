using PassKee.Business.Common.Utils;
using Xunit;

namespace PassKee.Tests.Unit.Utils;

public class StringUtilsTest
{
    [Theory]
    [InlineData("user@example.com", "user")]
    [InlineData("first.last@domain.org", "first.last")]
    [InlineData("USER@DOMAIN.COM", "user")]
    [InlineData("invalid-email", null)]
    [InlineData("", null)]
    [InlineData(null, null)]
    public void GetUserNameFromEmail_ShouldExtractUsername(string? email, string? expected)
    {
        var result = StringUtils.GetUserNameFromEmail(email!);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(0, "0B")]
    [InlineData(1024, "1KB")]
    [InlineData(1048576, "1MB")]
    [InlineData(1073741824, "1GB")]
    public void BytesToString_ShouldFormatCorrectly(long byteCount, string expected)
    {
        var result = StringUtils.BytesToString(byteCount);
        Assert.Equal(expected, result);
    }
}

