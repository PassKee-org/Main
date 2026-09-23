using System.ComponentModel.DataAnnotations;
using PassKee.Business.Common.Mvc.Attribute.Validation;

namespace PassKee.Tests.Unit.Mvc.Validation.Attribute;

public class IsEmailAddressAttributeTest
{
    private readonly IsEmailAddressAttribute _attribute = new();
    private readonly ValidationContext _context = new(new object()) { DisplayName = "Email" };

    [Theory]
    [InlineData("test@example.com")]
    [InlineData("user.name@domain.co")]
    [InlineData("user-name@domain.org")]
    [InlineData("user_name@domain.net")]
    [InlineData("user@domain.co.uk")]
    public void IsValid_WhenValidEmail_ShouldReturnSuccess(string email)
    {
        var result = _attribute.GetValidationResult(email, _context);

        Assert.Equal(ValidationResult.Success, result);
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("@missinguser.com")]
    [InlineData("user@")]
    [InlineData("user@.com")]
    [InlineData("user@domain..com")]
    [InlineData("user@domain.information")]
    public void IsValid_WhenInvalidEmail_ShouldReturnError(string email)
    {
        var result = _attribute.GetValidationResult(email, _context);

        Assert.NotNull(result);
        Assert.NotEqual(ValidationResult.Success, result);
    }

    [Fact]
    public void IsValid_WhenValueIsNull_ShouldReturnSuccess()
    {
        // RegularExpressionAttribute allows null (RequiredAttribute should be used in combination if required)
        var result = _attribute.GetValidationResult(null, _context);

        Assert.Equal(ValidationResult.Success, result);
    }
}

