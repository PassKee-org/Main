using System.ComponentModel.DataAnnotations;
using PassKee.Business.Common.Mvc.Attribute.Validation;

namespace PassKee.Tests.Unit.Business.Common.Mvc.Validation.Attribute;

public class StringArrayLengthAttributeTest
{
    private readonly ValidationContext _context = new(new object()) { DisplayName = "Tags" };

    [Fact]
    public void IsValid_WhenAllStringsWithinBounds_ShouldReturnSuccess()
    {
        var attribute = new StringArrayLengthAttribute(10);
        var array = new[] { "apple", "banana", "cherry" };

        var result = attribute.GetValidationResult(array, _context);

        Assert.Equal(ValidationResult.Success, result);
    }

    [Fact]
    public void IsValid_WhenStringExceedsMaxLength_ShouldReturnError()
    {
        var attribute = new StringArrayLengthAttribute(5);
        var array = new[] { "ok", "too_long_string" };

        var result = attribute.GetValidationResult(array, _context);

        Assert.NotNull(result);
        Assert.NotEqual(ValidationResult.Success, result);
    }

    [Fact]
    public void IsValid_WhenMinLengthSetAndStringIsTooShort_ShouldReturnError()
    {
        var attribute = new StringArrayLengthAttribute(10) { MinLength = 3 };
        var array = new[] { "valid", "ab" };

        var result = attribute.GetValidationResult(array, _context);

        Assert.NotNull(result);
        Assert.NotEqual(ValidationResult.Success, result);
    }

    [Fact]
    public void IsValid_WhenArrayContainsNull_ShouldReturnError()
    {
        var attribute = new StringArrayLengthAttribute(10);
        var array = new string?[] { "valid", null };

        var result = attribute.GetValidationResult(array, _context);

        Assert.NotNull(result);
        Assert.NotEqual(ValidationResult.Success, result);
    }

    [Fact]
    public void IsValid_WhenValueIsNull_ShouldReturnSuccess()
    {
        var attribute = new StringArrayLengthAttribute(10);

        var result = attribute.GetValidationResult(null, _context);

        Assert.Equal(ValidationResult.Success, result);
    }

    [Fact]
    public void IsValid_WhenValueIsNotStringArray_ShouldReturnSuccess()
    {
        var attribute = new StringArrayLengthAttribute(10);

        var result = attribute.GetValidationResult(12345, _context);

        Assert.Equal(ValidationResult.Success, result);
    }
}

