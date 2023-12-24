namespace Sharpify.Tests;
#pragma warning disable

public class StringExtensionsTests {
    [Fact]
    public void IsNullOrEmpty_GivenNullString_ReturnsTrue() {
        // Arrange
        string value = null;

        // Act
        var result = value.IsNullOrEmpty();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsNullOrEmpty_GivenEmptyString_ReturnsTrue() {
        // Arrange
        const string value = "";

        // Act
        var result = value.IsNullOrEmpty();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsNullOrWhiteSpace_GivenNullString_ReturnsTrue() {
        // Arrange
        string value = null;

        // Act
        var result = value.IsNullOrWhiteSpace();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsNullOrWhiteSpace_GivenEmptyString_ReturnsTrue() {
        // Arrange
        const string value = "";

        // Act
        var result = value.IsNullOrWhiteSpace();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsNullOrWhiteSpace_GivenWhiteSpaceString_ReturnsTrue() {
        // Arrange
        const string value = "      ";

        // Act
        var result = value.IsNullOrWhiteSpace();

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData("0", 0)]
    [InlineData("1", 1)]
    [InlineData("123", 123)]
    [InlineData("2147483647", 2147483647)] // int.MaxValue
    public void TryConvertToInt32_ValidString_ReturnsTrue(string input, int expected) {
        bool result = input.AsSpan().TryConvertToInt32(out var output);

        result.Should().BeTrue();
        output.Should().Be(expected);
    }

    [Theory]
    [InlineData("")]
    [InlineData("-1 5")] // whitespace
    [InlineData("214748364841232131231")] // larger than int.MaxValue
    [InlineData("1.23")] // decimal
    [InlineData("123abc")] // alphanumeric
    public void TryConvertToInt32_InvalidString_ReturnsFalse(string input) {
        bool result = input.AsSpan().TryConvertToInt32(out var output);

        result.Should().BeFalse();
        output.Should().Be(0); // Ensure that the value is not changed in case of failure
    }

    [Theory]
    [InlineData("", "", "")]
    [InlineData("hello", "", "hello")]
    [InlineData("", "world", "world")]
    [InlineData("hello", "world", "helloworld")]
    public void Suffix_WithVariousInputs_ReturnsCorrectResult(
        string value, string suffixString, string expectedResult) {
        // Arrange
        ReadOnlySpan<char> suffix = suffixString;

        // Act
        string result = value.Suffix(suffix);

        // Assert
        result.Should().Be(expectedResult);
    }

    [Fact]
    public void Suffix_WithLongSuffix_ReturnsCorrectResult() {
        // Arrange
        const string value = "prefix";
        string suffixString = new('a', 10000);
        string expectedResult = "prefix" + suffixString;
        ReadOnlySpan<char> suffix = suffixString;

        // Act
        string result = value.Suffix(suffix);

        // Assert
        result.Should().Be(expectedResult);
    }


    // Tests for Concat
    [Theory]
    [InlineData("", "", "")]
    [InlineData("hello", "", "hello")]
    [InlineData("", "world", "world")]
    [InlineData("hello", "world", "helloworld")]
    public void Concat_WithVariousInputs_ReturnsCorrectResult(
        string value, string suffixString, string expectedResult) {
        // Arrange
        ReadOnlySpan<char> suffix = suffixString;

        // Act
        string result = value.Concat(suffix);

        // Assert
        result.Should().Be(expectedResult);
    }

    // Tests for ToTitle
    [Theory]
    [InlineData("", "")]
    [InlineData("hello world", "Hello World")]
    public void ToTitle_WithVariousInputs_ReturnsTitleCase(
        string input, string expectedResult) {
        // Act
        string result = input.ToTitle();

        // Assert
        result.Should().Be(expectedResult);
    }

    // Tests for IsBinary
    [Theory]
    [InlineData("", true)]
    [InlineData("0", true)]
    [InlineData("1", true)]
    [InlineData("00 11\n\t01\r10", true)]
    [InlineData("0012", false)]
    [InlineData("hello", false)]
    public void IsBinary_WithVariousInputs_ReturnsCorrectResult(
        string input, bool expectedResult) {
        // Act
        bool result = input.IsBinary();

        // Assert
        result.Should().Be(expectedResult);
    }
}
#pragma warning restore