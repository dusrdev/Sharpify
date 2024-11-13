using Sharpify.Collections;

namespace Sharpify.Tests.Collections;

#if NET9_0_OR_GREATER
public class BufferWrapperTests {
    [Fact]
    public void BufferWrapper_NoCapacity_Throws() {
        // Arrange
        Action act = () => {
            var buffer = new BufferWrapper<char>();
            buffer.Append('a');
        };

        // Act & Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void BufferWrapper_Append_ToFullCapacity() {
        // Arrange
        const string text = "Hello world!";

        // Act
        Action act = () => {
            var buffer = BufferWrapper<char>.Create(new char[text.Length]);
            buffer.Append(text);
        };

        // Assert
        act.Should().NotThrow<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void BufferWrapper_Append_BeyondCapacity() {
        // Arrange
        const string text = "Hello world!";

        // Act
        Action act = () => {
            var buffer = BufferWrapper<char>.Create(new char[text.Length - 1]);
            buffer.Append(text);
        };

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void BufferWrapper_Reset() {
        // Arrange
        var buffer = BufferWrapper<char>.Create(new char[20]);

        // Act
        buffer.Append("Hello world!");
        buffer.Reset();
        buffer.Append("David");

        // Assert
        (buffer.WrittenSpan is "David").Should().BeTrue();
    }
}
#endif