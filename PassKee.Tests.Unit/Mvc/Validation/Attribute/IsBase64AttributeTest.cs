using System.ComponentModel.DataAnnotations;
using PassKee.Business.Common.Mvc.Attribute.Validation;

namespace PassKee.Tests.Unit.Mvc.Validation.Attribute;

public class IsBase64AttributeTest
{
    private readonly IsBase64Attribute _attribute = new();
    private readonly ValidationContext _context = new(new object()) { DisplayName = "Base64Field" };

    [Theory]
    [InlineData("SGVsbG8gV29ybGQ=")]
    [InlineData("AQIDBA==")]
    [InlineData("Zm9vYmFy")]
    public void IsValid_WhenValidBase64String_ShouldReturnSuccess(string validBase64)
    {
        var result = _attribute.GetValidationResult(validBase64, _context);

        Assert.Equal(ValidationResult.Success, result);
    }

    [Fact]
    public void IsValid_WhenValueIsNull_ShouldReturnSuccess()
    {
        var result = _attribute.GetValidationResult(null, _context);

        Assert.Equal(ValidationResult.Success, result);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-a-valid-base64")]
    [InlineData("SGVsbG8gV29ybGQ")] // missing padding
    [InlineData("%%%invalid%%%")]
    public void IsValid_WhenInvalidBase64String_ShouldReturnError(string invalidBase64)
    {
        var result = _attribute.GetValidationResult(invalidBase64, _context);

        Assert.NotNull(result);
        Assert.NotEqual(ValidationResult.Success, result);
    }

    [Fact]
    public void IsValid_WhenValueIsNotString_ShouldReturnError()
    {
        var result = _attribute.GetValidationResult(12345, _context);

        Assert.NotNull(result);
        Assert.NotEqual(ValidationResult.Success, result);
    }
}

