using Sharpify.Collections;

namespace Sharpify.Tests.Collections;

public class StringBuffersTests {
    [Fact]
    public void StringBuffer_NoCapacity_Throws() {
        // Arrange
        Action act = () => {
            var buffer = new StringBuffer();
            buffer.Append('a');
        };

        // Act & Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void StringBuffer_Append_ToFullCapacity() {
        // Arrange
        string text = "Hello world!";

        // Act
        Action act = () => {
            var buffer = StringBuffer.Create(stackalloc char[text.Length]);
            buffer.Append(text);
        };

        // Assert
        act.Should().NotThrow<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void StringBuffer_Append_BeyondCapacity() {
        // Arrange
        string text = "Hello world!";

        // Act
        Action act = () => {
            var buffer = StringBuffer.Create(stackalloc char[text.Length - 1]);
            buffer.Append(text);
        };

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void StringBuffer_Append_BeyondToCapacityAndBeyond() {
        // Arrange
        string text = "Hello world!";

        // Act

        Action act1 = () => {
            var buffer = StringBuffer.Create(stackalloc char[text.Length]);
            buffer.Append(text);
            buffer.Append(text);
        };
        Action act2 = () => {
            var buffer = StringBuffer.Create(stackalloc char[text.Length]);
            buffer.Append(text);
            buffer.Append(1);
        };
        Action act3 = () => {
            var buffer = StringBuffer.Create(stackalloc char[text.Length]);
            buffer.Append(text);
            buffer.Append('a');
        };

        // Assert
        act1.Should().Throw<ArgumentOutOfRangeException>();
        act2.Should().Throw<ArgumentOutOfRangeException>();
        act3.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void StringBuffer_AppendLine_OnElement() {
        // Arrange
        var buffer = StringBuffer.Create(stackalloc char[20]);

        // Act
        buffer.AppendLine("Hello");
        buffer.Append("World");

        var expected = string.Create(null, stackalloc char[20], $"Hello{Environment.NewLine}World");

        // Assert
        buffer.Allocate(true).Should().Be(expected);
    }

    [Fact]
    public void StringBuffer_AppendLine_NoParams() {
        // Arrange
        var buffer = StringBuffer.Create(stackalloc char[20]);

        // Act
        buffer.Append("Hello");
        buffer.AppendLine();
        buffer.Append("World");

        var expected = string.Create(null, stackalloc char[20], $"Hello{Environment.NewLine}World");

        // Assert
        buffer.Allocate(true).Should().Be(expected);
    }

    [Fact]
    public void StringBuffer_AppendLine_NoParams_Builder() {
        // Arrange
        var buffer = StringBuffer.Create(stackalloc char[20]);

        // Act
        buffer.Append("Hello")
              .AppendLine()
              .Append("World");

        var expected = string.Create(null, stackalloc char[20], $"Hello{Environment.NewLine}World");

        // Assert
        buffer.Allocate(true).Should().Be(expected);
    }

    [Fact]
    public void StringBuffer_NoTrimming_ReturnFullString() {
        // Arrange
        var buffer = StringBuffer.Create(stackalloc char[5]);

        // Act
        buffer.Append('a');
        buffer.Append('b');
        buffer.Append('c');
        buffer.Append('d');

        // Assert
        buffer.Allocate(false).Should().Be("abcd\0");
    }

    [Fact]
    public void StringBuffer_WithTrimming_ReturnTrimmedString() {
        // Arrange
        var buffer = StringBuffer.Create(stackalloc char[5]);

        // Act
        buffer.Append('a');
        buffer.Append('b');
        buffer.Append('c');
        buffer.Append('d');

        // Assert
        buffer.Allocate(true).Should().Be("abcd");
    }

    [Fact]
    public void StringBuffer_WithWhiteSpaceTrimming_ReturnTrimmedString() {
        // Arrange
        var buffer = StringBuffer.Create(stackalloc char[5]);

        // Act
        buffer.Append('a');
        buffer.Append('b');
        buffer.Append('c');
        buffer.Append('d');
        buffer.Append(' ');

        // Assert
        buffer.Allocate(true, true).Should().Be("abcd");
    }

    [Fact]
    public void StringBuffer_ImplicitOperatorString() {
        // Arrange
        var buffer = StringBuffer.Create(stackalloc char[10]);

        // Act
        buffer.Append('a');
        buffer.Append('b');
        buffer.Append('c');
        buffer.Append('d');

        // Assert
        string str = buffer.Allocate();
        str.Should().Be("abcd");
    }

    [Fact]
    public void StringBuffer_ImplicitOperatorReadOnlySpan() {
        // Arrange
        var buffer = StringBuffer.Create(stackalloc char[10]);

        // Act
        buffer.Append('a');
        buffer.Append('b');
        buffer.Append('c');
        buffer.Append('d');

        // Assert
        (buffer.WrittenSpan is "abcd").Should().Be(true);
    }
}