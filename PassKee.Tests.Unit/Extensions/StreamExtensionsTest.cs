using PassKee.Business.Extensions;
using Moq;

namespace PassKee.Tests.Unit.Extensions;

public class StreamExtensionsTest
{
    [Fact]
    public void PrepareToCopy_WhenReadable_ShouldSetPositionToZero()
    {
        using var stream = new MemoryStream([1, 2, 3, 4, 5]);
        stream.Position = 3;

        stream.PrepareToCopy();

        Assert.Equal(0, stream.Position);
    }

    [Fact]
    public void PrepareToCopy_WhenCannotRead_ShouldThrowNotSupportedException()
    {
        var mockStream = new Mock<Stream>();
        mockStream.SetupGet(s => s.CanRead).Returns(false);

        Assert.Throws<NotSupportedException>(() => mockStream.Object.PrepareToCopy());
    }
}

