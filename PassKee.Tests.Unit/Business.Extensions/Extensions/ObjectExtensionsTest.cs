using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using PassKee.Business.Extensions;

namespace PassKee.Tests.Unit.Business.Extensions;

public class ObjectExtensionsTest
{
    private enum SampleEnum
    {
        [Description("Sample Description")]
        SampleWithDesc,

        [Display(Name = "Sample Display")]
        SampleWithDisplay,

        SamplePlain
    }

    private class SampleClass
    {
        public string Name { get; set; } = string.Empty;
        public int Value { get; set; }
    }

    [Fact]
    public void EqualsToTypeName_WhenMatches_ShouldReturnTrue()
    {
        var obj = new SampleClass();

        Assert.True(obj.EqualsToTypeName("SampleClass"));
        Assert.True(obj.EqualsToTypeName("sampleclass"));
    }

    [Fact]
    public void EqualsToTypeName_WhenDoesNotMatch_ShouldReturnFalse()
    {
        var obj = new SampleClass();

        Assert.False(obj.EqualsToTypeName("OtherClass"));
    }

    [Fact]
    public void GetAsJson_ShouldSerializeObject()
    {
        var obj = new SampleClass { Name = "Test", Value = 42 };
        var json = obj.GetAsJson();

        Assert.Contains("\"Name\":\"Test\"", json);
        Assert.Contains("\"Value\":42", json);
    }

    [Fact]
    public void GetDisplayName_WithType_WhenHasDescription_ShouldReturnDescription()
    {
        var result = typeof(SampleEnum).GetDisplayName(SampleEnum.SampleWithDesc);
        Assert.Equal("Sample Description", result);
    }

    [Fact]
    public void GetDisplayName_WithType_WhenHasDisplay_ShouldReturnDisplayName()
    {
        var result = typeof(SampleEnum).GetDisplayName(SampleEnum.SampleWithDisplay);
        Assert.Equal("Sample Display", result);
    }

    [Fact]
    public void GetDisplayName_WithType_WhenNoAttribute_ShouldReturnName()
    {
        var result = typeof(SampleEnum).GetDisplayName(SampleEnum.SamplePlain);
        Assert.Equal("SamplePlain", result);
    }
}

