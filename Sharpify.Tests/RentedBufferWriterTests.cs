using Sharpify.Collections;

namespace Sharpify.Tests;

public class RentedBufferWriterTests {

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void RentedBufferWriter_InvalidCapacity_Throws(int capacity) {
        // Arrange
        Action act = () => {
            using var buffer = new RentedBufferWriter<char>(capacity);
        };

        // Act & Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void RentedBufferWriter_WriteToSpan() {
        // Arrange
        using var buffer = new RentedBufferWriter<char>(20);

        // Act
        var span = buffer.GetSpan();
        "Hello".AsSpan().CopyTo(span);
        buffer.Advance(5);

        // Assert
        buffer.WrittenSpan.SequenceEqual("Hello").Should().BeTrue();
    }

    [Fact]
    public void RentedBufferWriter_GetSpanSlice() {
        // Arrange
        using var buffer = new RentedBufferWriter<char>(20);

        // Act
        var span = buffer.GetSpan();
        "Hello".AsSpan().CopyTo(span);
        buffer.Advance(5);

        // Assert
        buffer.GetSpanSlice(0, 3).SequenceEqual("Hel").Should().BeTrue();
    }

    [Fact]
    public void RentedBufferWriter_WriteToMemory() {
        // Arrange
        using var buffer = new RentedBufferWriter<char>(20);

        // Act
        var mem = buffer.GetMemory();
        "Hello".AsSpan().CopyTo(mem.Span);
        buffer.Advance(5);

        // Assert
        buffer.WrittenSegment.SequenceEqual("Hello").Should().BeTrue();
    }

    [Fact]
    public void RentedBufferWriter_GetMemorySlice() {
        // Arrange
        using var buffer = new RentedBufferWriter<char>(20);

        // Act
        var mem = buffer.GetMemory();
        "Hello".AsSpan().CopyTo(mem.Span);
        buffer.Advance(5);

        // Assert
        buffer.GetMemorySlice(0, 3).Span.SequenceEqual("Hel").Should().BeTrue();
    }

    [Fact]
    public void RentedBufferWriter_WrittenSegment() {
        // Arrange
        using var buffer = new RentedBufferWriter<char>(20);

        // Act
        var span = buffer.GetSpan();
        "Hello".AsSpan().CopyTo(span);
        buffer.Advance(5);

        // Assert
        buffer.WrittenSegment.SequenceEqual("Hello").Should().BeTrue();
    }

    [Fact]
    public void RentedBufferWriter_Reset() {
        // Arrange
        using var buffer = new RentedBufferWriter<char>(20);

        // Act
        var span = buffer.GetSpan();
        "Hello".AsSpan().CopyTo(span);
        buffer.Advance(5);
        buffer.Reset();

        // Assert
        buffer.WrittenSpan.SequenceEqual(ReadOnlySpan<char>.Empty).Should().BeTrue();
    }

    [Fact]
    public void RentedBufferWriter_ActualCapacity() {
        // Arrange
        using var buffer = new RentedBufferWriter<char>(20);

        // Assert
        buffer.ActualCapacity.Should().BeGreaterThanOrEqualTo(20);
    }

    [Fact]
    public void RentedBufferWriter_FreeCapacity() {
        // Arrange
        using var buffer = new RentedBufferWriter<char>(20);

        // Act
        var span = buffer.GetSpan();
        "Hello".AsSpan().CopyTo(span);
        buffer.Advance(5);

        // Assert
        buffer.FreeCapacity.Should().BeGreaterThanOrEqualTo(15);
    }
}