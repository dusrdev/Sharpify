namespace Sharpify.Tests;
#pragma warning disable

public class StringExtensionsTests {
    // Tests for ToTitle
    [Theory]
    [InlineData("", "")]
    [InlineData("hello world", "Hello World")]
    public void ToTitle_WithVariousInputs_ReturnsTitleCase(
        string input, string expectedResult) {
        // Act
        string result = input.ToTitle();

        // Assert
        Assert.Equal(expectedResult, result);
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
        Assert.Equal(expectedResult, result);
    }
}
#pragma warning restore