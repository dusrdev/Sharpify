namespace Sharpify.Tests;

public partial class UtilsTests {
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