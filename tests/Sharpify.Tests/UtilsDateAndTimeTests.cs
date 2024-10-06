using System.Buffers;

namespace Sharpify.Tests;

public partial class UtilsTests {
    [Theory]
    [InlineData(0.00001, "0ms")]
    [InlineData(0.01, "10ms")]
    [InlineData(0.5, "500ms")]
    [InlineData(1, "01:000s")]
    [InlineData(59.99, "59:990s")]
    [InlineData(60, "01:00m")]
    [InlineData(61, "01:01m")]
    [InlineData(121.234, "02:01m")]
    public void FormatTimeSpan_ReturnsFormattedSpan(double seconds, string expected) {
        // Arrange
        var elapsed = TimeSpan.FromSeconds(seconds);
        using var owner = MemoryPool<char>.Shared.Rent(30);

        // Act
        ReadOnlySpan<char> result = Utils.DateAndTime.FormatTimeSpan(elapsed, owner.Memory.Span);

        // Assert
        result.SequenceEqual(expected).Should().BeTrue();
    }

    [Theory]
    [InlineData(0.00001, "0ms")]
    [InlineData(0.01, "10ms")]
    [InlineData(0.5, "500ms")]
    [InlineData(1, "01:000s")]
    [InlineData(59.99, "59:990s")]
    [InlineData(60, "01:00m")]
    [InlineData(61, "01:01m")]
    [InlineData(121.234, "02:01m")]
    public void FormatTimeSpan_ReturnsFormattedString(double seconds, string expected) {
        // Arrange
        var elapsed = TimeSpan.FromSeconds(seconds);

        // Act
        string result = Utils.DateAndTime.FormatTimeSpan(elapsed);

        // Assert
        result.Equals(expected).Should().BeTrue();
    }

    [Fact]
    public void ToTimeStamp_ReturnsFormattedSpan() {
        // Arrange
        var dateTime = new DateTime(2022, 04, 06, 13, 55, 00);
        using var owner = MemoryPool<char>.Shared.Rent(30);

        // Act
        ReadOnlySpan<char> result = Utils.DateAndTime.FormatTimeStamp(dateTime, owner.Memory.Span);

        // Assert
        result.SequenceEqual("1355-6-Apr-22").Should().BeTrue();
    }

    [Fact]
    public void ToTimeStamp_ReturnsFormattedString() {
        // Arrange
        var dateTime = new DateTime(2022, 04, 06, 13, 55, 00);

        // Act
        string result = Utils.DateAndTime.FormatTimeStamp(dateTime);

        // Assert
        result.Equals("1355-6-Apr-22").Should().BeTrue();
    }

    [Fact]
    public async Task GetCurrentTimeAsync_ReturnsCurrentTime() {
        // Arrange
        var expected = DateTime.Now;

        // Act
        var result = await Utils.DateAndTime.GetCurrentTimeAsync();

        // Assert
        result.Should().BeCloseTo(expected, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task GetCurrentTimeInBinaryAsync_ReturnsCurrentTimeInBinary() {
        // Arrange
        var expected = DateTime.Now;

        // Act
        var result = await Utils.DateAndTime.GetCurrentTimeInBinaryAsync();
        var fromResult = DateTime.FromBinary(result);

        // Assert
        fromResult.Should().BeCloseTo(expected, TimeSpan.FromSeconds(1));
    }
}