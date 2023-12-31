namespace Sharpify.Collections;

/// <summary>
/// Represents a mutable string buffer that allows efficient appending of characters, strings and other <see cref="ISpanFormattable"/> implementations. This version requires you to preallocate the buffer, allowing stackalloc usage.
/// </summary>
public ref struct AllocatedStringBuffer {
    private readonly Span<char> _buffer;
    private readonly int _length;
    private int _position;

    /// <summary>
    /// Represents a mutable interface over a buffer allocated in memory.
    /// </summary>
    public AllocatedStringBuffer(Span<char> buffer) {
        _buffer = buffer;
        _length = _buffer.Length;
        _position = 0;
    }

    /// <summary>
    /// This returns an empty buffer. It will throw if you try to append anything to it. use <see cref="AllocatedStringBuffer(Span{char})"/> instead.
    /// </summary>
    public AllocatedStringBuffer() : this(Span<char>.Empty) {
    }

    /// <summary>
    /// Represents a buffer efficient concatenation of strings and other types on a preallocated buffer.
    /// </summary>
    public static AllocatedStringBuffer Create(Span<char> buffer) => new(buffer);

    /// <summary>
    /// Appends a character to the string buffer.
    /// </summary>
    /// <param name="c">The character to append.</param>
    public void Append(char c) {
#if NET8_0_OR_GREATER
        ArgumentOutOfRangeException.ThrowIfGreaterThan(_position + 1, _length);
#elif NET7_0
        if (_position + 1 > _length) {
            throw new ArgumentOutOfRangeException(nameof(_length));
        }
#endif

        _buffer[_position++] = c;
    }

    /// <summary>
    /// Appends the specified string to the buffer.
    /// </summary>
    /// <param name="str">The string to append.</param>
    public void Append(ReadOnlySpan<char> str) {
#if NET8_0_OR_GREATER
        ArgumentOutOfRangeException.ThrowIfGreaterThan(_position + str.Length, _length);
#elif NET7_0
        if (_position + str.Length > _length) {
            throw new ArgumentOutOfRangeException(nameof(_length));
        }
#endif

        str.CopyTo(_buffer[_position..]);
        _position += str.Length;
    }

    /// <summary>
    /// Appends a value to the string buffer, using the specified format and format provider.
    /// </summary>
    /// <typeparam name="T">The type of the value to append.</typeparam>
    /// <param name="value">The value to append.</param>
    /// <param name="format">The format specifier to apply to the value.</param>
    /// <param name="provider">The format provider to use.</param>
    /// <exception cref="InvalidOperationException">Thrown when the buffer is full.</exception>
    public void Append<T>(T value, ReadOnlySpan<char> format = default, IFormatProvider? provider = null) where T : ISpanFormattable {
        var written = value.TryFormat(_buffer[_position..], out var charsWritten, format, provider);
        if (!written) {
            throw new ArgumentOutOfRangeException(nameof(_length));
        }

        _position += charsWritten;
    }

    /// <summary>
    /// Allocates a string from the internal buffer.
    /// </summary>
    /// <param name="trimIfShorter">Indicates whether to trim the string from the end.</param>
    /// <param name="trimEndWhiteSpace">Will try to trim the end white spaces</param>
    /// <returns>The allocated string.</returns>
    public readonly string Allocate(bool trimIfShorter = true, bool trimEndWhiteSpace = false) {
        ReadOnlySpan<char> span = _buffer;
        if (trimIfShorter) {
            span = span[0.._position];
        }
        if (trimEndWhiteSpace) {
            span = span.TrimEnd();
        }
        return new string(span);
    }


    /// <summary>
    /// Allocates a substring from the internal buffer using the specified range.
    /// </summary>
    public readonly string this[Range range] => Allocate(range);

    /// <summary>
    /// Allocates a substring from the internal buffer using the specified range.
    /// </summary>
    /// <param name="range"></param>
    private readonly string Allocate(Range range) {
        ReadOnlySpan<char> span = _buffer[range];
        return new string(span);
    }

    /// <summary>
    /// Use the allocate function with the trimEnd parameter set to true.
    /// </summary>
    /// <param name="buffer"></param>
    public static implicit operator string(AllocatedStringBuffer buffer) => buffer.Allocate(true, false);

    /// <summary>
    /// Returns a string allocated from the StringBuffer.
    /// </summary>
    /// <remarks>It is identical to <see cref="Allocate(bool, bool)"/></remarks>
    public override readonly string ToString() => Allocate(true, false);
}