namespace Sharpify.Tests;

public partial class UtilsTests {
    [Theory]
    [InlineData(0.0, "0 B")]
    [InlineData(1023.0, "1023 B")]
    [InlineData(1024.0, "1 KB")]
    [InlineData(1057.393, "1.03 KB")]
    [InlineData(1048576.0, "1 MB")]
    [InlineData(1073741824.0, "1 GB")]
    [InlineData(1099511627776.0, "1 TB")]
    [InlineData(1125899906842624.0, "1 PB")]
    public void FormatBytes_DoubleWithVariousInputs_ReturnsCorrectResult(
        double bytes, string expectedResult) {
        // Act
        string result = Utils.Strings.FormatBytes(bytes);

        // Assert
        result.Should().Be(expectedResult);
    }

    [Theory]
    [InlineData(0L, "0 B")]
    [InlineData(1023L, "1023 B")]
    [InlineData(1024L, "1 KB")]
    [InlineData(1048576L, "1 MB")]
    [InlineData(1073741824L, "1 GB")]
    [InlineData(1099511627776L, "1 TB")]
    [InlineData(1125899906842624L, "1 PB")]
    public void FormatBytes_LongWithVariousInputs_ReturnsCorrectResult(
        long bytes, string expectedResult) {
        // Act
        string result = Utils.Strings.FormatBytes(bytes);

        // Assert
        result.Should().Be(expectedResult);
    }

    [Theory]
    [InlineData(1610612736L, "1.5 GB")] // 1.5 * 1024^3
    [InlineData(1627389952L, "1.52 GB")] // 1.52 * 1024^3
    [InlineData(1644167168L, "1.53 GB")] // 1.53 * 1024^3
    public void FormatBytes_LongWithNonRoundedInputs_ReturnsCorrectResult(
        long bytes, string expectedResult) {
        // Act
        string result = Utils.Strings.FormatBytes(bytes);

        // Assert
        result.Should().Be(expectedResult);
    }

    [Theory]
    [InlineData(1610612736.0, "1.5 GB")] // 1.5 * 1024^3
    [InlineData(1627389952.0, "1.52 GB")] // 1.52 * 1024^3
    [InlineData(1644167168.0, "1.53 GB")] // 1.53 * 1024^3
    public void FormatBytes_DoubleWithNonRoundedInputs_ReturnsCorrectResult(
        double bytes, string expectedResult) {
        // Act
        string result = Utils.Strings.FormatBytes(bytes);

        // Assert
        result.Should().Be(expectedResult);
    }
}