using PassKee.Web.Core.Ui.Shared.Components.Dropdown;
using Xunit;

#pragma warning disable BL0005

namespace PassKee.Tests.Unit.Web.Core.Ui.Shared.Components;

public class AppDropdownTest
{
    private class TestableDropdown : AppDropdown
    {
        public string GetPositionClass() => PositionClass;
    }

    [Fact]
    public void AppDropdown_WhenIsDropUpTrue_ReturnsBottomFullPosition()
    {
        var dropdown = new TestableDropdown { IsDropUp = true };
        var positionClass = dropdown.GetPositionClass();
        Assert.Contains("bottom-full", positionClass);
        Assert.Contains("mb-1.5", positionClass);
        Assert.DoesNotContain("top-full", positionClass);
    }

    [Fact]
    public void AppDropdown_WhenIsDropUpFalse_ReturnsTopFullPosition()
    {
        var dropdown = new TestableDropdown { IsDropUp = false };
        var positionClass = dropdown.GetPositionClass();
        Assert.Contains("top-full", positionClass);
        Assert.Contains("mt-1.5", positionClass);
        Assert.DoesNotContain("bottom-full", positionClass);
    }

    [Fact]
    public void AppDropdown_WhenIsInlineTrue_ReturnsRelativePosition()
    {
        var dropdown = new TestableDropdown { IsInline = true, IsDropUp = true };
        var positionClass = dropdown.GetPositionClass();
        Assert.Contains("relative", positionClass);
        Assert.DoesNotContain("absolute", positionClass);
    }
}
