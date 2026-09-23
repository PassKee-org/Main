using System.Drawing;

namespace PassKee.Business.Extensions;

public static class ColorExtensions
{
    public static string? ToHexString(this Color c)
    {
        if (c == Color.Empty)
        {
            return null;
        }

        return $"#{c.R:X2}{c.G:X2}{c.B:X2}";
    }

    public static string? ToHexString(this Color? c)
    {
        if (c == null)
        {
            return null;
        }
        return c?.ToHexString();
    }

    public static string ToRgbString(this Color c) => $"RGB({c.R}, {c.G}, {c.B})";
    
    public static Color GetTextColorBasedOn(this Color? c)
    {
        if (c == null)
        {
            return Color.Empty;
        }
        return c?.GetTextColorBasedOn() ?? Color.Empty;
    }
    
    public static Color GetTextColorBasedOn(this Color c)
    {
        var l = 0.2126 * c.R + 0.7152 * c.G + 0.0722 * c.B;
        return l < 0.5 ? Color.White : Color.Black;
    }
}
