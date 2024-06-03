namespace Sharpify.Tests;

public class DateAndTimeExtensionsTests {
    [Theory]
    [InlineData(0.00001, "0.01ms")]
    [InlineData(0.01, "10ms")]
    [InlineData(0.5, "500ms")]
    [InlineData(1, "1s")]
    [InlineData(59.99, "59.99s")]
    [InlineData(60, "1m")]
    [InlineData(61, "1.02m")]
    [InlineData(121.234, "2.02m")]
    public void Format_GivenTimeSpan_ReturnsFormattedString(double seconds, string expected) {
        // Arrange
        var elapsed = TimeSpan.FromSeconds(seconds);

        // Act
        var result = elapsed.Format();

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(1, 2, 3, 4, 567, "1d 2h 3m 4s")]
    [InlineData(1, 2, 3, 0, 0, "1d 2h 3m")]
    [InlineData(1, 2, 0, 4, 0, "1d 2h 4s")]
    [InlineData(0, 2, 0, 4, 0, "2h 4s")]
    public void ToRemainingDuration_GivenTimeSpan_ReturnsHumanReadableFormat(int days, int hours, int minutes, int seconds, int milliseconds, string expected) {
        // Arrange
        var time = new TimeSpan(days, hours, minutes, seconds, milliseconds);

        // Act
        var result = time.ToRemainingDuration();

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void ToRemainingDuration_GivenTimeSpanLessThanOneSecond_ReturnsZeroSeconds() {
        // Arrange
        var time = TimeSpan.FromMilliseconds(500);

        // Act
        var result = time.ToRemainingDuration();

        // Assert
        result.Should().Be("0s");
    }

    [Fact]
    public void ToTimeStamp_GivenDateTime_ReturnsTimeStamp() {
        // Arrange
        var dateTime = new DateTime(2022, 04, 06, 13, 55, 00);

        // Act
        var result = dateTime.ToTimeStamp();

        // Assert
        result.Should().Be("1355-6-Apr-22");
    }
}