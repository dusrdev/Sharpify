namespace Sharpify.Tests;

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

    [Fact]
    public void ConvertToInt32_GivenValidString_ReturnsInt32Value() {
        // Arrange
        const string value = "1234";

        // Act
        var result = value.ConvertToInt32();

        // Assert
        result.Should().Be(1234);
    }

    [Fact]
    public void ConvertToInt32_GivenNegativeString_ReturnsNegativeInt32Value() {
        // Arrange
        const string value = "-1234";

        // Act
        var result = value.ConvertToInt32();

        // Assert
        result.Should().Be(-1234);
    }

    [Fact]
    public void ConvertToInt32_GivenEmptyString_ReturnsZero() {
        // Arrange
        const string value = "";

        // Act
        var result = value.ConvertToInt32();

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public void ConvertToInt32_GivenInvalidString_ReturnsZero() {
        // Arrange
        const string value = "12-34";

        // Act
        var result = value.ConvertToInt32();

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public void ConvertToInt32_GivenStringExceedingMaxValue_ReturnsZero() {
        // Arrange
        const string value = "2147483648";

        // Act
        var result = value.ConvertToInt32();

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public void ConvertToInt32Unsigned_GivenValidSpan_ShouldSucceed() {
        // Arrange
        var span = "1234".AsSpan();
        var num = 0;

        // Act
        span.ConvertToInt32Unsigned(ref num);

        // Assert
        num.Should().Be(1234);
    }

    [Fact]
    public void ConvertToInt32Unsigned_GivenEmptySpan_ShouldSetResultToNegativeOne() {
        // Arrange
        var span = ReadOnlySpan<char>.Empty;
        var num = 0;

        // Act
        span.ConvertToInt32Unsigned(ref num);

        // Assert
        num.Should().Be(-1);
    }

    [Fact]
    public void ConvertToInt32Unsigned_GivenSpanWithNonDigitChars_ShouldSetResultToNegativeOne() {
        // Arrange
        var span = "12-34".AsSpan();
        var num = 0;

        // Act
        span.ConvertToInt32Unsigned(ref num);

        // Assert
        num.Should().Be(-1);
    }

    [Fact]
    public void ConvertToInt32Unsigned_GivenSpanExceedingMaxValue_ShouldSetResultToNegativeOne() {
        // Arrange
        var span = "2147483648".AsSpan();
        var num = 0;

        // Act
        span.ConvertToInt32Unsigned(ref num);

        // Assert
        num.Should().Be(-1);
    }

    [Fact]
    public void ConvertsToInt32Unsigned_GivenValidSpan_ShouldSucceed() {
        // Arrange
        var span = "1234".AsSpan();

        // Act
        var result = span.ConvertsToInt32Unsigned();

        // Assert
        result.Should().Be(1234);
    }

    [Fact]
    public void ConvertsToInt32Unsigned_GivenEmptySpan_ShouldReturnNegativeOne() {
        // Arrange
        var span = ReadOnlySpan<char>.Empty;

        // Act
        var result = span.ConvertsToInt32Unsigned();

        // Assert
        result.Should().Be(-1);
    }

    [Fact]
    public void ConvertsToInt32Unsigned_GivenSpanWithNonDigitChars_ShouldReturnNegativeOne() {
        // Arrange
        var span = "12-34".AsSpan();

        // Act
        var result = span.ConvertsToInt32Unsigned();

        // Assert
        result.Should().Be(-1);
    }

    [Fact]
    public void ConvertsToInt32Unsigned_GivenSpanExceedingMaxValue_ShouldReturnNegativeOne() {
        // Arrange
        var span = "2147483648".AsSpan();

        // Act
        var result = span.ConvertsToInt32Unsigned();

        // Assert
        result.Should().Be(-1);
    }

    public class Tests {
        [Fact]
        public void ConvertToInt32Unsigned_PositiveIntegerString_ReturnsExpectedValue() {
            // Arrange
            var str = "123";

            // Act
            var result = str.ConvertToInt32Unsigned();

            // Assert
            Assert.Equal(123, result);
        }

        [Fact]
        public void ConvertToInt32Unsigned_EmptyString_ReturnsNegativeOne() {
            // Arrange
            var str = string.Empty;

            // Act
            var result = str.ConvertToInt32Unsigned();

            // Assert
            result.Should().Be(-1);
        }

        [Fact]
        public void ConvertToInt32Unsigned_InvalidIntegerString_ReturnsNegativeOne() {
            // Arrange
            var str = "123a";

            // Act
            var result = str.ConvertToInt32Unsigned();

            // Assert
            result.Should().Be(-1);
        }
    }

    [Theory]
    [InlineData("0", 0)]
    [InlineData("1", 1)]
    [InlineData("123", 123)]
    [InlineData("2147483647", 2147483647)] // int.MaxValue
    public void TryConvertToInt32_ValidString_ReturnsTrue(string input, int expected) {
        int output = 0;
        bool result = input.TryConvertToInt32(ref output);

        result.Should().BeTrue();
        output.Should().Be(expected);
    }

    [Theory]
    [InlineData("")]
    [InlineData("-1")]
    [InlineData("2147483648")] // int.MaxValue + 1
    [InlineData("1.23")]
    [InlineData("123abc")]
    public void TryConvertToInt32_InvalidString_ReturnsFalse(string input) {
        int output = 0;
        bool result = input.TryConvertToInt32(ref output);

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