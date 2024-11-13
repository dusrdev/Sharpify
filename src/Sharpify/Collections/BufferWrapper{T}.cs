using System.Buffers;

namespace Sharpify.Collections;

#if NET9_0_OR_GREATER
/// <summary>
/// Represents a buffer than be used to efficiently append items to a span.
/// </summary>
public ref struct BufferWrapper<T> : IBufferWriter<T> {
    private readonly Span<T> _buffer;

    /// <summary>
    /// The total length of the buffer.
    /// </summary>
    public readonly int Length;

    /// <summary>
    /// The current position of the buffer.
    /// </summary>
    public int Position { get; private set; }

    /// <summary>
    /// Initializes a string buffer that uses a pre-allocated buffer (potentially from the stack).
    /// </summary>
    public static BufferWrapper<T> Create(Span<T> buffer) => new(buffer);

    /// <summary>
    /// Represents a mutable interface over a buffer allocated in memory.
    /// </summary>
    private BufferWrapper(Span<T> buffer) {
        _buffer = buffer;
        Length = _buffer.Length;
        Position = 0;
    }

    /// <summary>
    /// This returns an empty buffer. It will throw if you try to append anything to it. use <see cref="BufferWrapper{T}.Create(Span{T})"/> instead.
    /// </summary>
    public BufferWrapper() : this(Span<T>.Empty) {
    }

    /// <summary>
    /// Resets the buffer to the beginning.
    /// </summary>
    public void Reset() => Position = 0;

    /// <summary>
    /// Appends an item to the end of the buffer.
    /// </summary>
    public void Append(T item) {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(Position + 1, Length);
        _buffer[Position++] = item;
    }

    /// <summary>
    /// Appends the span of items to the end of the buffer.
    /// </summary>
    public void Append(ReadOnlySpan<T> items) {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(Position + items.Length, Length);
        items.CopyTo(_buffer.Slice(Position));
        Position += items.Length;
    }

    /// <inheritdoc/>
    public void Advance(int count) => Position += count;

    /// <inheritdoc/>
    public Memory<T> GetMemory(int sizeHint = 0) => throw new NotSupportedException("BufferWrapper does not support GetMemory");

    /// <inheritdoc/>
    public Span<T> GetSpan(int sizeHint = 0) => _buffer.Slice(Position);

    /// <summary>
    /// Returns the character at the specified index.
    /// </summary>
    /// <param name="index"></param>
    public readonly T this[int index] => _buffer[index];

    /// <summary>
    /// Returns the used portion of the buffer as a readonly span.
    /// </summary>
    public readonly ReadOnlySpan<T> WrittenSpan => _buffer.Slice(0, Position);
}
#endif