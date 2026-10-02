using System.ComponentModel.DataAnnotations;
using PassKee.Business.Common.Mvc.Attribute.Validation;

namespace PassKee.Tests.Unit.Business.Common.Mvc.Validation.Attribute;

public class IsPositiveAttributeTest
{
    private readonly ValidationContext _context = new(new object()) { DisplayName = "PositiveNumber" };

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    public void IsValid_WhenIntPositive_ShouldReturnSuccess(int value)
    {
        var attribute = new IsPositiveAttribute();
        var result = attribute.GetValidationResult(value, _context);

        Assert.Equal(ValidationResult.Success, result);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void IsValid_WhenIntNonPositive_ShouldReturnError(int value)
    {
        var attribute = new IsPositiveAttribute();
        var result = attribute.GetValidationResult(value, _context);

        Assert.NotNull(result);
        Assert.NotEqual(ValidationResult.Success, result);
    }

    [Fact]
    public void IsValid_WhenLongPositive_ShouldReturnSuccess()
    {
        var attribute = new IsPositiveAttribute();
        var result = attribute.GetValidationResult(100L, _context);

        Assert.Equal(ValidationResult.Success, result);
    }

    [Fact]
    public void IsValid_WhenDecimalPositive_ShouldReturnSuccess()
    {
        var attribute = new IsPositiveAttribute();
        var result = attribute.GetValidationResult(0.5m, _context);

        Assert.Equal(ValidationResult.Success, result);
    }

    [Fact]
    public void IsValid_WhenFloatPositive_ShouldReturnSuccess()
    {
        var attribute = new IsPositiveAttribute();
        var result = attribute.GetValidationResult(1.5f, _context);

        Assert.Equal(ValidationResult.Success, result);
    }

    [Fact]
    public void IsValid_WhenDoublePositive_ShouldReturnSuccess()
    {
        var attribute = new IsPositiveAttribute();
        var result = attribute.GetValidationResult(2.5d, _context);

        Assert.Equal(ValidationResult.Success, result);
    }

    [Fact]
    public void IsValid_WhenAllowZeroTrueAndValueIsZero_ShouldReturnSuccess()
    {
        var attribute = new IsPositiveAttribute { AllowZero = true };
        var result = attribute.GetValidationResult(0, _context);

        Assert.Equal(ValidationResult.Success, result);
    }

    [Fact]
    public void IsValid_WhenAllowZeroTrueAndValueIsNegative_ShouldReturnError()
    {
        var attribute = new IsPositiveAttribute { AllowZero = true };
        var result = attribute.GetValidationResult(-5, _context);

        Assert.NotNull(result);
        Assert.NotEqual(ValidationResult.Success, result);
    }

    [Fact]
    public void IsValid_WhenValueIsNull_ShouldReturnSuccess()
    {
        var attribute = new IsPositiveAttribute();
        var result = attribute.GetValidationResult(null, _context);

        Assert.Equal(ValidationResult.Success, result);
    }

    [Fact]
    public void IsValid_WhenValueIsNotNumeric_ShouldReturnError()
    {
        var attribute = new IsPositiveAttribute();
        var result = attribute.GetValidationResult("not a number", _context);

        Assert.NotNull(result);
        Assert.NotEqual(ValidationResult.Success, result);
    }
}

