using System.Net;

namespace PassKee.Web.Core.Utils;

public static class EmojiHelper
{
    /// <summary>
    /// Formats an emoji or HTML symbol icon.
    /// If the string contains HTML entities (such as &#128512; or &hearts;), it is decoded into Unicode characters.
    /// Returns null if the value is null or whitespace.
    /// </summary>
    public static string? FormatEmoji(string? icon)
    {
        if (string.IsNullOrWhiteSpace(icon))
        {
            return null;
        }

        var trimmed = icon.Trim();
        return trimmed.Contains('&') ? WebUtility.HtmlDecode(trimmed).Trim() : trimmed;
    }
}

