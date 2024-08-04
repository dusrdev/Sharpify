using Sharpify.Collections;

namespace Sharpify.Tests.Collections;

public class StringBuffersTests {
    [Fact]
    public void StringBuffer_NoCapacity_Throws() {
        // Arrange
        Action act = () => {
            using var buffer = new StringBuffer();
            buffer.Append('a');
        };

        // Act & Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void StringBuffer_AppendLine_OnElement() {
        // Arrange
        using var buffer = StringBuffer.Rent(20);

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
        using var buffer = StringBuffer.Rent(20);

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
        using var buffer = StringBuffer.Rent(20);

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
        using var buffer = StringBuffer.Rent(5, true);

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
        using var buffer = StringBuffer.Rent(5, true);

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
        using var buffer = StringBuffer.Rent(5, true);

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
    public void StringBuffer_AllocateWithIndexes() {
        // Arrange
        using var buffer = StringBuffer.Rent(4);

        // Act
        buffer.Append('a');
        buffer.Append('b');
        buffer.Append('c');
        buffer.Append('d');

        // Assert
        buffer[1..^1].Should().Be("bc");
    }

    [Fact]
    public void StringBuffer_ImplicitOperatorString() {
        // Arrange
        using var buffer = StringBuffer.Rent(4);

        // Act
        buffer.Append('a');
        buffer.Append('b');
        buffer.Append('c');
        buffer.Append('d');

        // Assert
        string str = buffer;
        str.Should().Be("abcd");
    }

    [Fact]
    public void StringBuffer_ImplicitOperatorReadOnlySpan() {
        // Arrange
        using var buffer = StringBuffer.Rent(4);

        // Act
        buffer.Append('a');
        buffer.Append('b');
        buffer.Append('c');
        buffer.Append('d');

        // Assert
        ReadOnlySpan<char> span = buffer;
        span.SequenceEqual("abcd").Should().Be(true);
    }

    [Fact]
    public void StringBuffer_ImplicitOperatorReadOnlyMemory() {
        // Arrange
        using var buffer = StringBuffer.Rent(4);

        // Act
        buffer.Append('a');
        buffer.Append('b');
        buffer.Append('c');
        buffer.Append('d');

        // Assert
        ReadOnlyMemory<char> span = buffer;
        span.Span.SequenceEqual("abcd").Should().Be(true);
    }
}