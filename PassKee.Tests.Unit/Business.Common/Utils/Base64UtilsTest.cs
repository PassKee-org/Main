using System;
using PassKee.Business.Common.Utils;
using Xunit;

namespace PassKee.Tests.Unit.Business.Common.Utils;

public class Base64UtilsTest
{
    [Theory]
    [InlineData("aGVsbG8gd29ybGQ=", true)]
    [InlineData("YW55IGNhcm5hbCBwbGVhc3VyZS4=", true)]
    [InlineData("YW55IGNhcm5hbCBwbGVhc3VyZQ==", true)]
    [InlineData("YW55IGNhcm5hbCBwbGVhc3Vy", true)]
    [InlineData("", false)]
    [InlineData(null, false)]
    [InlineData("not a valid base 64 string!?", false)]
    [InlineData("===invalid", false)]
    public void IsValidBase64_ShouldValidateCorrectly(string? input, bool expected)
    {
        var result = Base64Utils.IsValidBase64(input!);
        Assert.Equal(expected, result);
    }
}

