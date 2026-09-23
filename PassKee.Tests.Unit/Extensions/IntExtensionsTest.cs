using PassKee.Business.Extensions;

namespace PassKee.Tests.Unit.Extensions;

public class IntExtensionsTest
{
    [Theory]
    [InlineData(2, 0, 1)]
    [InlineData(2, 1, 2)]
    [InlineData(2, 3, 8)]
    [InlineData(5, 3, 125)]
    [InlineData(0, 5, 0)]
    public void Pow_ShouldCalculatePower(int number, uint power, int expected)
    {
        Assert.Equal(expected, number.Pow(power));
    }

    [Theory]
    [InlineData(1, true)]
    [InlineData(100, true)]
    [InlineData(0, false)]
    [InlineData(-1, false)]
    public void IsPositive_Int_ShouldReturnExpected(int number, bool expected)
    {
        Assert.Equal(expected, number.IsPositive());
    }

    [Fact]
    public void IsPositive_NullableInt_WhenNull_ShouldReturnFalse()
    {
        int? number = null;
        Assert.False(number.IsPositive());
    }

    [Fact]
    public void IsPositive_NullableInt_WhenHasValue_ShouldReturnExpected()
    {
        int? positive = 5;
        int? zero = 0;
        int? negative = -5;

        Assert.True(positive.IsPositive());
        Assert.False(zero.IsPositive());
        Assert.False(negative.IsPositive());
    }

    [Theory]
    [InlineData(1L, true)]
    [InlineData(100L, true)]
    [InlineData(0L, false)]
    [InlineData(-1L, false)]
    public void IsPositive_Long_ShouldReturnExpected(long number, bool expected)
    {
        Assert.Equal(expected, number.IsPositive());
    }

    [Fact]
    public void IsPositive_NullableLong_WhenNull_ShouldReturnFalse()
    {
        long? number = null;
        Assert.False(number.IsPositive());
    }

    [Fact]
    public void IsPositive_NullableLong_WhenHasValue_ShouldReturnExpected()
    {
        long? positive = 50L;
        long? zero = 0L;
        long? negative = -50L;

        Assert.True(positive.IsPositive());
        Assert.False(zero.IsPositive());
        Assert.False(negative.IsPositive());
    }
}

