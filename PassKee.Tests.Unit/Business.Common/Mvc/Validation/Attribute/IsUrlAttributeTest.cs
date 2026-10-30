using System.ComponentModel.DataAnnotations;
using PassKee.Business.Common.Mvc.Attribute.Validation;

namespace PassKee.Tests.Unit.Business.Common.Mvc.Validation.Attribute;

public class IsUrlAttributeTest
{
    private readonly IsUrlAttribute _attribute = new();
    private readonly ValidationContext _context = new(new object()) { DisplayName = "Url" };

    [Theory]
    [InlineData("http://localhost")]
    [InlineData("https://example.com")]
    [InlineData("http://example.com/path/to/page")]
    [InlineData("https://sub.domain.org/test/path")]
    [InlineData("https://sub.domain.org:8080/test/path")]
    [InlineData("http://192.168.1.1")]
    public void IsValid_WhenValidUrl_ShouldReturnSuccess(string url)
    {
        var result = _attribute.GetValidationResult(url, _context);

        Assert.Equal(ValidationResult.Success, result);
    }

    [Theory]
    [InlineData("ftp://example.com")]
    [InlineData("not-a-url")]
    [InlineData("http://")]
    [InlineData("https://")]
    [InlineData("mailto:user@example.com")]
    public void IsValid_WhenInvalidUrl_ShouldReturnError(string url)
    {
        var result = _attribute.GetValidationResult(url, _context);

        Assert.NotNull(result);
        Assert.NotEqual(ValidationResult.Success, result);
    }

    [Fact]
    public void IsValid_WhenValueIsNull_ShouldReturnSuccess()
    {
        var result = _attribute.GetValidationResult(null, _context);

        Assert.Equal(ValidationResult.Success, result);
    }
}
