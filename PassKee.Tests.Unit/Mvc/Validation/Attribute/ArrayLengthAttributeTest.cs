using System.ComponentModel.DataAnnotations;
using PassKee.Business.Common.Mvc.Attribute.Validation;

namespace PassKee.Tests.Unit.Mvc.Validation.Attribute;

public class ArrayLengthAttributeTest
{
    [Fact]
    public void IsValid_WhenCountLessThanOrEqualMax_ShouldBeValid()
    {
        var attribute = new ArrayLengthAttribute(3);
        var list = new List<string> { "a", "b", "c" };
        var context = new ValidationContext(new object()) { DisplayName = "Items" };

        var result = attribute.GetValidationResult(list, context);

        Assert.Equal(ValidationResult.Success, result);
    }

    [Fact]
    public void IsValid_WhenCountGreaterThanMax_ShouldReturnError()
    {
        var attribute = new ArrayLengthAttribute(2);
        var list = new List<string> { "a", "b", "c" };
        var context = new ValidationContext(new object()) { DisplayName = "Items" };

        var result = attribute.GetValidationResult(list, context);

        Assert.NotNull(result);
        Assert.NotEqual(ValidationResult.Success, result);
    }

    [Fact]
    public void IsValid_WhenValueIsNull_ShouldBeValid()
    {
        var attribute = new ArrayLengthAttribute(2);
        var context = new ValidationContext(new object()) { DisplayName = "Items" };

        var result = attribute.GetValidationResult(null, context);

        Assert.Equal(ValidationResult.Success, result);
    }

    [Fact]
    public void IsValid_WhenValueIsNotList_ShouldBeValid()
    {
        var attribute = new ArrayLengthAttribute(2);
        var context = new ValidationContext(new object()) { DisplayName = "Items" };

        var result = attribute.GetValidationResult("not a list", context);

        Assert.Equal(ValidationResult.Success, result);
    }
}

