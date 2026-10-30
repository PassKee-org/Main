using System.ComponentModel.DataAnnotations;
using PassKee.Business.Common.Mvc.Attribute.Validation;

namespace PassKee.Tests.Unit.Business.Common.Mvc.Validation.Attribute;

public class RequiredNonEmptyAttributeTest
{
    private readonly RequiredNonEmptyAttribute _attribute = new();
    private readonly ValidationContext _context = new(new object()) { DisplayName = "RequiredField", MemberName = "RequiredField" };

    [Fact]
    public void IsValid_WhenValueIsNull_ShouldReturnError()
    {
        var result = _attribute.GetValidationResult(null, _context);

        Assert.NotNull(result);
        Assert.NotEqual(ValidationResult.Success, result);
    }

    [Fact]
    public void IsValid_WhenGuidIsEmpty_ShouldReturnError()
    {
        var result = _attribute.GetValidationResult(Guid.Empty, _context);

        Assert.NotNull(result);
        Assert.NotEqual(ValidationResult.Success, result);
    }

    [Fact]
    public void IsValid_WhenGuidIsNotEmpty_ShouldReturnSuccess()
    {
        var result = _attribute.GetValidationResult(Guid.NewGuid(), _context);

        Assert.Equal(ValidationResult.Success, result);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t\n")]
    public void IsValid_WhenStringIsEmptyOrWhitespace_ShouldReturnError(string value)
    {
        var result = _attribute.GetValidationResult(value, _context);

        Assert.NotNull(result);
        Assert.NotEqual(ValidationResult.Success, result);
    }

    [Fact]
    public void IsValid_WhenStringIsNonEmpty_ShouldReturnSuccess()
    {
        var result = _attribute.GetValidationResult("valid string", _context);

        Assert.Equal(ValidationResult.Success, result);
    }

    [Fact]
    public void IsValid_WhenNonStringObjectProvided_ShouldReturnSuccess()
    {
        var result = _attribute.GetValidationResult(123, _context);

        Assert.Equal(ValidationResult.Success, result);
    }
}

