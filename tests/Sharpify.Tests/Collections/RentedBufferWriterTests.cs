using Sharpify.Collections;

namespace Sharpify.Tests.Collections;

public class RentedBufferWriterTests {
    [Fact]
    public void RentedBufferWriter_InvalidCapacity_Throws() {
        // Arrange
        Action act = () => {
            using var buffer = new RentedBufferWriter<char>(-1);
        };

        // Act & Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void RentedBufferWriter_Capacity0IsDisabled() {
        // Arrange
        using var buffer = new RentedBufferWriter<char>(0);

        // Assert
        buffer.IsDisabled.Should().BeTrue();
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
    public void RentedBufferWriter_WriteAndAdvance() {
        // Arrange
        using var buffer = new RentedBufferWriter<char>(20);

        // Act
        buffer.WriteAndAdvance("Hello");

        // Assert
        buffer.WrittenSpan.SequenceEqual("Hello").Should().BeTrue();
    }

    [Fact]
    public void RentedBufferWriter_UseRefToWriteValue() {
        // Arrange
        using var buffer = new RentedBufferWriter<int>(20);

        // Act
        ref var arr = ref buffer.GetReferenceUnsafe();
        var length = WriteOnes(ref arr, 5);
        buffer.Advance(length);

        // Assert
        buffer.WrittenSpan.SequenceEqual([1, 1, 1, 1, 1]).Should().BeTrue();

        static int WriteOnes(ref int[] buffer, int length) {
            for (var i = 0; i < length; i++) {
                buffer[i] = 1;
            }

            return length;
        }
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