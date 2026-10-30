using System.ComponentModel.DataAnnotations;
using Moq;
using PassKee.Business.Common.Mvc.Attribute.Validation;
using PassKee.Business.Common.Services.Web.ReCaptcha;

namespace PassKee.Tests.Unit.Business.Common.Mvc.Validation.Attribute;

public class IsReCaptchaAttributeTest
{
    private readonly IsReCaptchaAttribute _attribute = new();

    [Fact]
    public void IsValid_WhenNoReCaptchaServiceRegistered_ShouldReturnSuccess()
    {
        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(sp => sp.GetService(typeof(IReCaptchaService))).Returns(null!);
        var context = new ValidationContext(new object(), serviceProviderMock.Object, null) { DisplayName = "ReCaptcha" };

        var result = _attribute.GetValidationResult("some-token", context);

        Assert.Equal(ValidationResult.Success, result);
    }

    [Fact]
    public void IsValid_WhenReCaptchaServiceValidatesToken_ShouldReturnSuccess()
    {
        var reCaptchaMock = new Mock<IReCaptchaService>();
        reCaptchaMock.Setup(s => s.ValidateAsync("valid-token")).ReturnsAsync(true);

        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(sp => sp.GetService(typeof(IReCaptchaService))).Returns(reCaptchaMock.Object);
        var context = new ValidationContext(new object(), serviceProviderMock.Object, null) { DisplayName = "ReCaptcha" };

        var result = _attribute.GetValidationResult("valid-token", context);

        Assert.Equal(ValidationResult.Success, result);
    }

    [Fact]
    public void IsValid_WhenReCaptchaServiceRejectsToken_ShouldReturnError()
    {
        var reCaptchaMock = new Mock<IReCaptchaService>();
        reCaptchaMock.Setup(s => s.ValidateAsync("invalid-token")).ReturnsAsync(false);

        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(sp => sp.GetService(typeof(IReCaptchaService))).Returns(reCaptchaMock.Object);
        var context = new ValidationContext(new object(), serviceProviderMock.Object, null) { DisplayName = "ReCaptcha" };

        var result = _attribute.GetValidationResult("invalid-token", context);

        Assert.NotNull(result);
        Assert.NotEqual(ValidationResult.Success, result);
    }

    [Fact]
    public void IsValid_WhenTokenIsNullAndServiceRegistered_ShouldReturnError()
    {
        var reCaptchaMock = new Mock<IReCaptchaService>();
        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(sp => sp.GetService(typeof(IReCaptchaService))).Returns(reCaptchaMock.Object);
        var context = new ValidationContext(new object(), serviceProviderMock.Object, null) { DisplayName = "ReCaptcha" };

        var result = _attribute.GetValidationResult(null, context);

        Assert.NotNull(result);
        Assert.NotEqual(ValidationResult.Success, result);
    }

    [Fact]
    public void IsValid_WhenTokenIsNotStringAndServiceRegistered_ShouldReturnError()
    {
        var reCaptchaMock = new Mock<IReCaptchaService>();
        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(sp => sp.GetService(typeof(IReCaptchaService))).Returns(reCaptchaMock.Object);
        var context = new ValidationContext(new object(), serviceProviderMock.Object, null) { DisplayName = "ReCaptcha" };

        var result = _attribute.GetValidationResult(12345, context);

        Assert.NotNull(result);
        Assert.NotEqual(ValidationResult.Success, result);
    }
}
