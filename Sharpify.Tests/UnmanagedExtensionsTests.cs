namespace Sharpify.Tests;

public class UnmanagedExtensionsTests {
        public enum ExampleEnum {
        FirstValue,
        SecondValue,
        ThirdValue
    }

    [Theory]
    [InlineData("FirstValue", true, ExampleEnum.FirstValue)]
    [InlineData("SecondValue", true, ExampleEnum.SecondValue)]
    [InlineData("ThirdValue", true, ExampleEnum.ThirdValue)]
    [InlineData("InvalidValue", false, default(ExampleEnum))]
    [InlineData("", false, default(ExampleEnum))]
    public void TryParseAsEnum_WithVariousInputs_ReturnsCorrectResult(
        string value, bool expectedResult, ExampleEnum expectedEnum) {
        // Act
        bool result = value.TryParseAsEnum(out ExampleEnum parsedEnum);

        // Assert
        result.Should().Be(expectedResult);
        parsedEnum.Should().Be(expectedEnum);
    }
}