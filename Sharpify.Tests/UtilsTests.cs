namespace Sharpify.Tests;

public class UtilsTests {
    [Fact]
    public async Task GetCurrentTimeAsync_ReturnsCurrentTime() {
        // Arrange
        var expected = DateTime.Now;

        // Act
        var result = await GetCurrentTimeAsync();

        // Assert
        result.Should().BeCloseTo(expected, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task GetCurrentTimeInBinaryAsync_ReturnsCurrentTimeInBinary() {
        // Arrange
        var expected = DateTime.Now;

        // Act
        var result = await GetCurrentTimeInBinaryAsync();
        var fromResult = DateTime.FromBinary(result);

        // Assert
        fromResult.Should().BeCloseTo(expected, TimeSpan.FromSeconds(1));
    }

    [Theory]
    [InlineData(0, 5, 2, 2.5)]
    [InlineData(20, 10, 2, 15)]
    [InlineData(30, 30, 3, 30)]
    public void RollingAverage_WithVariousInputs_ReturnsCorrectResult(
        double val, double newVal, int count, double expectedResult)
    {
        // Arrange
        expectedResult = Math.Round(expectedResult, 15);

        // Act
        double result = RollingAverage(val, newVal, count);
        result = Math.Round(result, 15);

        // Assert
        result.Should().Be(expectedResult);
    }

    [Fact]
    public void RollingAverage_WithNegativeCount_ThrowsArgumentException()
    {
        // Arrange
        const double val = 10;
        const double newVal = 15;
        const int count = -1;

        // Act
        Action act = () => RollingAverage(val, newVal, count);

        // Assert
        act.Should().Throw<ArgumentException>();
    }
}