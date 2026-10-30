using PassKee.Business.Extensions;

namespace PassKee.Tests.Unit.Business.Extensions;

public class ByteExtensionsTest
{
    [Fact]
    public void GetString_ShouldReturnDecodedString()
    {
        var bytes = System.Text.Encoding.Default.GetBytes("Hello World");
        var result = bytes.GetString();

        Assert.Equal("Hello World", result);
    }

    [Fact]
    public void ToHexString_WhenNull_ShouldReturnNull()
    {
        byte[]? bytes = null;
        var result = bytes.ToHexString();

        Assert.Null(result);
    }

    [Fact]
    public void ToHexString_WhenEmpty_ShouldReturnEmptyString()
    {
        var bytes = Array.Empty<byte>();
        var result = bytes.ToHexString();

        Assert.Equal(string.Empty, result);
    }

    [Theory]
    [InlineData(new byte[] { 0x0A, 0x1B, 0xFF }, "0A1BFF")]
    [InlineData(new byte[] { 0x00, 0x7F }, "007F")]
    public void ToHexString_ShouldReturnHexRepresentation(byte[] bytes, string expected)
    {
        var result = bytes.ToHexString();

        Assert.Equal(expected, result);
    }

    [Fact]
    public void CompareTo_WhenEitherIsNull_ShouldReturnFalse()
    {
        byte[]? a = null;
        byte[] b = [1, 2, 3];

        Assert.False(a.CompareTo(b));
        Assert.False(b.CompareTo(a));
    }

    [Fact]
    public void CompareTo_WhenDifferentLengths_ShouldReturnFalse()
    {
        byte[] a = [1, 2];
        byte[] b = [1, 2, 3];

        Assert.False(a.CompareTo(b));
    }

    [Fact]
    public void CompareTo_WhenDifferentContent_ShouldReturnFalse()
    {
        byte[] a = [1, 2, 3];
        byte[] b = [1, 2, 4];

        Assert.False(a.CompareTo(b));
    }

    [Fact]
    public void CompareTo_WhenIdentical_ShouldReturnTrue()
    {
        byte[] a = [1, 2, 3];
        byte[] b = [1, 2, 3];

        Assert.True(a.CompareTo(b));
    }
}

