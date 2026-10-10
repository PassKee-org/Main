using PassKee.Web.Core.Ui.Shared.Components.Emoji;
using PassKee.Web.Core.Utils;
using Xunit;

namespace PassKee.Tests.Unit.Web.Core.Utils;

public class EmojiHelperTests
{
    [Theory]
    [InlineData("😀", "😀")]
    [InlineData("🔥", "🔥")]
    [InlineData("🚀", "🚀")]
    [InlineData("  🔑  ", "🔑")]
    public void FormatEmoji_WhenUnicodeEmojiProvided_ReturnsTrimmedEmoji(string input, string expected)
    {
        var result = EmojiHelper.FormatEmoji(input);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("&#128512;", "😀")]
    [InlineData("&#128274;", "🔒")]
    [InlineData("&#128273;", "🔑")]
    [InlineData("&#128179;", "💳")]
    [InlineData("  &#128293;  ", "🔥")]
    public void FormatEmoji_WhenDecimalHtmlEntityProvided_DecodesToUnicodeEmoji(string input, string expected)
    {
        var result = EmojiHelper.FormatEmoji(input);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("&#x1F600;", "😀")]
    [InlineData("&#x1F512;", "🔒")]
    [InlineData("&#x1F511;", "🔑")]
    public void FormatEmoji_WhenHexHtmlEntityProvided_DecodesToUnicodeEmoji(string input, string expected)
    {
        var result = EmojiHelper.FormatEmoji(input);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t\n")]
    public void FormatEmoji_WhenNullOrWhitespace_ReturnsNull(string? input)
    {
        var result = EmojiHelper.FormatEmoji(input);
        Assert.Null(result);
    }

    [Fact]
    public void EmojiList_ContainsAllExpectedCategoriesAndValidEntries()
    {
        Assert.NotEmpty(EmojiList.List);
        Assert.True(EmojiList.List.Count >= 400);

        foreach (var emoji in EmojiList.List)
        {
            Assert.False(string.IsNullOrWhiteSpace(emoji.Symbol), $"Emoji symbol should not be empty: {emoji.Name}");
            Assert.False(string.IsNullOrWhiteSpace(emoji.Name), "Emoji name should not be empty");
            Assert.False(string.IsNullOrWhiteSpace(emoji.HtmlCode), $"Emoji HtmlCode should not be empty: {emoji.Name}");
            Assert.False(string.IsNullOrWhiteSpace(emoji.Category), $"Emoji category should not be empty: {emoji.Name}");
            Assert.False(string.IsNullOrWhiteSpace(emoji.Keywords), $"Emoji keywords should not be empty: {emoji.Name}");
        }
    }
}
