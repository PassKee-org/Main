using System.ComponentModel.DataAnnotations;
using PassKee.Business.Common.Mvc.Attribute.Validation;

namespace PassKee.Tests.Unit.Business.Common.Mvc.Validation.Attribute;

public class ValidateListModelsAttributeTest
{
    private class TestItemModel
    {
        [Required]
        public string? Name { get; set; }

        [Range(1, 100)]
        public int Age { get; set; }
    }

    private readonly ValidateListModelsAttribute _attribute = new();
    private readonly ValidationContext _context = new(new object()) { DisplayName = "ItemList" };

    [Fact]
    public void IsValid_WhenAllItemsValid_ShouldReturnSuccess()
    {
        var list = new List<TestItemModel>
        {
            new() { Name = "Alice", Age = 25 },
            new() { Name = "Bob", Age = 30 }
        };

        var result = _attribute.GetValidationResult(list, _context);

        Assert.Equal(ValidationResult.Success, result);
    }

    [Fact]
    public void IsValid_WhenAnyItemInvalid_ShouldReturnError()
    {
        var list = new List<TestItemModel>
        {
            new() { Name = "Alice", Age = 25 },
            new() { Name = null, Age = 30 } // Name is required
        };

        var result = _attribute.GetValidationResult(list, _context);

        Assert.NotNull(result);
        Assert.NotEqual(ValidationResult.Success, result);
    }

    [Fact]
    public void IsValid_WhenListIsEmpty_ShouldReturnSuccess()
    {
        var list = new List<TestItemModel>();

        var result = _attribute.GetValidationResult(list, _context);

        Assert.Equal(ValidationResult.Success, result);
    }

    [Fact]
    public void IsValid_WhenValueIsNull_ShouldReturnSuccess()
    {
        var result = _attribute.GetValidationResult(null, _context);

        Assert.Equal(ValidationResult.Success, result);
    }

    [Fact]
    public void IsValid_WhenValueIsNotEnumerable_ShouldReturnSuccess()
    {
        var result = _attribute.GetValidationResult("not a collection", _context);

        Assert.Equal(ValidationResult.Success, result);
    }
}

