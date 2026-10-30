using System.Drawing;
using PassKee.Business.Extensions;

namespace PassKee.Tests.Unit.Business.Extensions;

public class ColorExtensionsTest
{
    [Fact]
    public void ToHexString_WhenColorEmpty_ShouldReturnNull()
    {
        var result = Color.Empty.ToHexString();
        Assert.Null(result);
    }

    [Fact]
    public void ToHexString_WhenValidColor_ShouldReturnHex()
    {
        var color = Color.FromArgb(255, 128, 0);
        var result = color.ToHexString();

        Assert.Equal("#FF8000", result);
    }

    [Fact]
    public void ToHexString_Nullable_WhenNull_ShouldReturnNull()
    {
        Color? color = null;
        Assert.Null(color.ToHexString());
    }

    [Fact]
    public void ToHexString_Nullable_WhenValue_ShouldReturnHex()
    {
        Color? color = Color.FromArgb(0, 0, 255);
        Assert.Equal("#0000FF", color.ToHexString());
    }

    [Fact]
    public void ToRgbString_ShouldReturnRgbFormat()
    {
        var color = Color.FromArgb(10, 20, 30);
        Assert.Equal("RGB(10, 20, 30)", color.ToRgbString());
    }

    [Fact]
    public void GetTextColorBasedOn_WhenNull_ShouldReturnColorEmpty()
    {
        Color? color = null;
        Assert.Equal(Color.Empty, color.GetTextColorBasedOn());
    }

    [Fact]
    public void GetTextColorBasedOn_WhenPureBlack_ShouldReturnWhite()
    {
        var black = Color.FromArgb(0, 0, 0);
        Assert.Equal(Color.White, black.GetTextColorBasedOn());
    }

    [Fact]
    public void GetTextColorBasedOn_WhenColorHasLuminanceAboveHalf_ShouldReturnBlack()
    {
        var white = Color.FromArgb(255, 255, 255);
        Assert.Equal(Color.Black, white.GetTextColorBasedOn());

        var red = Color.FromArgb(255, 0, 0);
        Assert.Equal(Color.Black, red.GetTextColorBasedOn());
    }
}

