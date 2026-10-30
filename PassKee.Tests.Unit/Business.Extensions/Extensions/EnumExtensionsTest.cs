using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using PassKee.Business.Extensions;

namespace PassKee.Tests.Unit.Business.Extensions;

public class EnumExtensionsTest
{
    public enum TestEnum
    {
        [Description("Description Value")]
        WithDescription,

        [Display(Name = "Display Name Value")]
        WithDisplay,

        PlainValue
    }

    [Fact]
    public void GetDisplayName_WhenHasDescriptionAttribute_ShouldReturnDescription()
    {
        var value = TestEnum.WithDescription;
        Assert.Equal("Description Value", value.GetDisplayName());
    }

    [Fact]
    public void GetDisplayName_WhenHasDisplayAttribute_ShouldReturnDisplayName()
    {
        var value = TestEnum.WithDisplay;
        Assert.Equal("Display Name Value", value.GetDisplayName());
    }

    [Fact]
    public void GetDisplayName_WhenNoAttribute_ShouldReturnEnumName()
    {
        var value = TestEnum.PlainValue;
        Assert.Equal("PlainValue", value.GetDisplayName());
    }
}

