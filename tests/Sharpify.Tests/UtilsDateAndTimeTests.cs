using System.Buffers;

namespace Sharpify.Tests;

public partial class UtilsTests {
    [Fact]
    public void ToTimeStamp_ReturnsFormattedSpan() {
        // Arrange
        var dateTime = new DateTime(2022, 04, 06, 13, 55, 00);
        using var owner = MemoryPool<char>.Shared.Rent(30);

        // Act
        ReadOnlySpan<char> result = Utils.FormatTimeStamp(dateTime, owner.Memory.Span);

        // Assert
        Assert.Equal("1355-6-Apr-22", result);
    }

    [Fact]
    public void ToTimeStamp_ReturnsFormattedString() {
        // Arrange
        var dateTime = new DateTime(2022, 04, 06, 13, 55, 00);

        // Act
        string result = Utils.FormatTimeStamp(dateTime);

        // Assert
        Assert.Equal("1355-6-Apr-22", result);
    }
}