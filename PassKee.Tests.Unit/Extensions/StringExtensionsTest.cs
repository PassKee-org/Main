using PassKee.Business.Extensions;

namespace PassKee.Tests.Unit.Extensions;

public class StringExtensionsTest
{
    [Fact]
    public void CountLines_ShouldCalculateNewLine1()
    {
        Assert.Equal(3, $"asd{Environment.NewLine}aaaaa {Environment.NewLine} ".CountLines());
    }

    [Fact]
    public void CountLines_ShouldCalculateNewLine2()
    {
        Assert.Equal(2, $"asd{Environment.NewLine}aaaaa".CountLines());
    }

    [Fact]
    public void CountLines_ShouldCalculateNewLine3()
    {
        Assert.Equal(1, "2023-10-30T00:00:00.0000000".CountLines());
    }

    [Fact]
    public void CountLines_ShouldCalculateNewLine4()
    {
        Assert.Equal(0, string.Empty.CountLines());
    }

    [Fact]
    public void CountLines_WhenNull_ShouldThrowArgumentNullException()
    {
        string? str = null;
        Assert.Throws<ArgumentNullException>(() => str!.CountLines());
    }

    [Fact]
    public void ToByteArray_And_GetUtf8Bytes_ShouldReturnBytes()
    {
        var text = "Hello World";
        Assert.Equal(System.Text.Encoding.Default.GetBytes(text), text.ToByteArray());
        Assert.Equal(System.Text.Encoding.UTF8.GetBytes(text), text.GetUtf8Bytes());
    }

    [Fact]
    public void ToHexBytes_WhenNull_ShouldReturnNull()
    {
        string? hex = null;
        Assert.Null(hex.ToHexBytes());
    }

    [Fact]
    public void ToHexBytes_WhenEmpty_ShouldReturnEmptyArray()
    {
        Assert.Empty("".ToHexBytes()!);
    }

    [Fact]
    public void ToHexBytes_WhenValidHex_ShouldReturnByteArray()
    {
        var result = "0A1BFF".ToHexBytes();
        Assert.Equal([0x0A, 0x1B, 0xFF], result);
    }

    [Fact]
    public void HexStringToByteArray_WhenOddLength_ShouldThrowArgumentException()
    {
        Assert.Throws<ArgumentException>(() => "123".HexStringToByteArray());
    }

    [Fact]
    public void HexStringToByteArray_WhenValid_ShouldReturnBytes()
    {
        var bytes = "0A1B".HexStringToByteArray();
        Assert.Equal([0x0A, 0x1B], bytes);
    }

    [Fact]
    public void ToHexString_And_FromHexString_ShouldRoundTrip()
    {
        var original = "PassKee";
        var hex = original.ToHexString();
        var roundTrip = hex.FromHexString();

        Assert.Equal(original, roundTrip);
    }

    [Fact]
    public void CleanToLowedString_ShouldKeepOnlyLettersAndDigits()
    {
        var input = "Hello, World! 123 #$%";
        var result = input.CleanToLowedString();

        Assert.Equal("HelloWorld123", result);
    }

    [Fact]
    public void StripHTML_ShouldRemoveHtmlTags()
    {
        var html = "<p>Hello <b>World</b></p>";
        var result = html.StripHTML();

        Assert.Equal("Hello World", result);
    }

    [Fact]
    public void GetMBtcString_ShouldFormatDecimal()
    {
        decimal amount = 1.2345m;
        Assert.Equal("1.2345 mBTC", amount.GetMBtcString());
    }

    [Fact]
    public void Truncate_ShouldTruncateWhenExceedsMaxLength()
    {
        var str = "abcdefghij";
        Assert.Equal("abcde", str.Truncate(5));
        Assert.Equal("abcdefghij", str.Truncate(20));
        Assert.Null(((string?)null).Truncate(5));
    }

    [Fact]
    public void Truncate_WithDotsOption_ShouldAppendDots()
    {
        var str = "abcdefghij";
        Assert.Equal("abcde...", str.Truncate(5, isAddDots: true));
        Assert.Equal("abcde", str.Truncate(5, isAddDots: false));
    }

    [Fact]
    public void TruncateAndAddDots_ShouldAddDots()
    {
        var str = "abcdefghij";
        Assert.Equal("abcde...", str.TruncateAndAddDots(5));
        Assert.Equal("abcdefghij", str.TruncateAndAddDots(15));
    }

    [Fact]
    public void TrimLastSlash_ShouldRemoveTrailingSlashOnly()
    {
        Assert.Equal("http://example.com/api", "http://example.com/api/".TrimLastSlash());
        Assert.Equal("http://example.com/api", "http://example.com/api".TrimLastSlash());
    }

    [Fact]
    public void MySubstring_ShouldNotExceedLength()
    {
        var str = "abcdef";
        Assert.Equal("abc", str.MySubstring(0, 3));
        Assert.Equal("cdef", str.MySubstring(2, 10));
    }

    [Theory]
    [InlineData("123.45", 123.45)]
    [InlineData("123,45", 123.45)]
    [InlineData("1 234.56", 1234.56)]
    [InlineData("invalid", 0)]
    public void ToDecimal_ShouldParseVariousFormats(string input, decimal expected)
    {
        Assert.Equal(expected, input.ToDecimal());
    }

    [Fact]
    public void SlashHelpers_ShouldEnsureOrRemoveCorrectly()
    {
        Assert.Equal("/path", "path".EnsureLeadingSlash());
        Assert.Equal("/path", "/path".EnsureLeadingSlash());
        Assert.Null(((string?)null).EnsureLeadingSlash());

        Assert.Equal("path/", "path".EnsureTrailingSlash());
        Assert.Equal("path/", "path/".EnsureTrailingSlash());
        Assert.Null(((string?)null).EnsureTrailingSlash());

        Assert.Equal("path", "/path".RemoveLeadingSlash());
        Assert.Equal("path", "path".RemoveLeadingSlash());
        Assert.Null(((string?)null).RemoveLeadingSlash());

        Assert.Equal("path", "path/".RemoveTrailingSlash());
        Assert.Equal("path", "path".RemoveTrailingSlash());
        Assert.Null(((string?)null).RemoveTrailingSlash());

        Assert.Equal("path", "/path".RemoveLeadingPathSeparator());
        Assert.Equal("path", "\\path".RemoveLeadingPathSeparator());
        Assert.Null(((string?)null).RemoveLeadingPathSeparator());

        Assert.Equal("/", "".CleanUrlPath());
        Assert.Equal("/api", "/api/".CleanUrlPath());
        Assert.Equal("/", "/".CleanUrlPath());
    }

    [Fact]
    public void ToUpperFirstChar_ShouldCapitalizeFirstLetterOfEachWord()
    {
        Assert.Equal("Hello World", "hello world".ToUpperFirstChar());
    }

    [Fact]
    public void FirstCharToUpper_ShouldCapitalizeFirstLetter()
    {
        Assert.Equal("Hello", "hello".FirstCharToUpper());
        Assert.Equal("Hello", "hELLO".FirstCharToUpper());
        Assert.Throws<ArgumentNullException>(() => ((string?)null)!.FirstCharToUpper());
        Assert.Throws<ArgumentException>(() => string.Empty.FirstCharToUpper());
    }

    [Fact]
    public void FirstChar_ShouldReturnFirstCharacterOrEmpty()
    {
        Assert.Equal("a", "abc".FirstChar());
        Assert.Equal("", "".FirstChar());
        Assert.Equal("", ((string?)null).FirstChar());
    }

    [Fact]
    public void RemoveNewLineSymbols_ShouldRemoveWhitespaceLines()
    {
        var str = "line1\r\n\tline2";
        Assert.Equal("line1line2", str.RemoveNewLineSymbols());
    }

    [Fact]
    public void RemoveNewLineSymbolsWithChars_ShouldNormalizeEscapedNewlines()
    {
        var str = "line1\\nline2";
        Assert.Equal("line1\nline2", str.RemoveNewLineSymbolsWithChars());
    }

    [Fact]
    public void GetFirstUpperLetters_ShouldReturnInitials()
    {
        Assert.Equal("JD", "John Doe".GetFirstUpperLetters(2));
        Assert.Equal("J", "John Doe".GetFirstUpperLetters(1));
        Assert.Throws<ArgumentNullException>(() => ((string?)null)!.GetFirstUpperLetters());
    }
}
