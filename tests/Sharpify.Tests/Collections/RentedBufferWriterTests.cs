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
        Assert.Throws<ArgumentOutOfRangeException>(act);
    }

    [Fact]
    public void RentedBufferWriter_Capacity0IsDisabled() {
        // Arrange
        using var buffer = new RentedBufferWriter<char>(0);

        // Assert
        Assert.True(buffer.IsDisabled);
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
        Assert.Equal("Hello", buffer.WrittenSpan);
    }

    [Fact]
    public void RentedBufferWriter_WriteAndAdvance() {
        // Arrange
        using var buffer = new RentedBufferWriter<char>(20);

        // Act
        buffer.WriteAndAdvance("Hello");

        // Assert
        Assert.Equal("Hello", buffer.WrittenSpan);
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
        Assert.Equal([1, 1, 1, 1, 1], buffer.WrittenSpan);

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
        Assert.Equal("Hel", buffer.GetSpanSlice(0, 3));
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
        Assert.Equal("Hello".ToCharArray(), buffer.WrittenSegment);
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
        Assert.Equal("Hello", buffer.GetMemorySlice(0, 5).Span);
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
        Assert.Equal("Hello".ToCharArray(), buffer.WrittenSegment);
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
        Assert.Equal(ReadOnlySpan<char>.Empty, buffer.WrittenSpan);
    }

    [Fact]
    public void RentedBufferWriter_ActualCapacity() {
        // Arrange
        using var buffer = new RentedBufferWriter<char>(20);

        // Assert
        Assert.True(buffer.ActualCapacity >= 20);
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
        Assert.True(buffer.FreeCapacity >= 15);
    }
}