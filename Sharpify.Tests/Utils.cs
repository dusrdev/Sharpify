namespace Sharpify.Tests;

public class Utils {
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
        var expected = DateTime.Now.ToBinary();

        // Act
        var result = await GetCurrentTimeInBinaryAsync();

        // Assert
        result.Should().Be(expected);
    }
}