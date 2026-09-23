using System.ComponentModel.DataAnnotations;
using PassKee.Business.Common.Mvc.Attribute.Validation;

namespace PassKee.Tests.Unit.Mvc.Validation.Attribute;

public class IsIPv4AddressTest
{
    private readonly IsIPv4Address _attribute = new();
    private readonly ValidationContext _context = new(new object()) { DisplayName = "IpAddress" };

    [Theory]
    [InlineData("127.0.0.1")]
    [InlineData("192.168.1.1")]
    [InlineData("0.0.0.0")]
    [InlineData("255.255.255.255")]
    [InlineData("10.0.0.1")]
    public void IsValid_WhenValidIPv4_ShouldReturnSuccess(string ip)
    {
        var result = _attribute.GetValidationResult(ip, _context);

        Assert.Equal(ValidationResult.Success, result);
    }

    [Theory]
    [InlineData("256.0.0.1")]
    [InlineData("1.2.3.4.5")]
    [InlineData("1.2.3")]
    [InlineData("01.1.1.1")]
    [InlineData("a.b.c.d")]
    [InlineData("1.2.3.-1")]
    [InlineData("...")]
    [InlineData("hello")]
    public void IsValid_WhenInvalidIPv4_ShouldReturnError(string ip)
    {
        var result = _attribute.GetValidationResult(ip, _context);

        Assert.NotNull(result);
        Assert.NotEqual(ValidationResult.Success, result);
    }

    [Fact]
    public void IsValid_WhenValueIsNull_ShouldReturnSuccess()
    {
        var result = _attribute.GetValidationResult(null, _context);

        Assert.Equal(ValidationResult.Success, result);
    }

    [Fact]
    public void IsValid_WhenValueIsNotString_ShouldReturnError()
    {
        var result = _attribute.GetValidationResult(127001, _context);

        Assert.NotNull(result);
        Assert.NotEqual(ValidationResult.Success, result);
    }
}

