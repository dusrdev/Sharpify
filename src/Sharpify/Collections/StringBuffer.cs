using System.Buffers;
using System.Runtime.CompilerServices;

namespace Sharpify.Collections;

/// <summary>
/// Represents a mutable string buffer that allows efficient appending of characters, strings and other <see cref="ISpanFormattable"/> implementations.
/// </summary>
public unsafe ref partial struct StringBuffer {
    private static readonly string NewLine = Environment.NewLine;
    private readonly char[] _source;
    private readonly Span<char> _buffer;

    /// <summary>
    /// The total length of the buffer.
    /// </summary>
    public readonly int Length;

    private int _position;
    private volatile bool _disposed;

    /// <summary>
    /// Creates a mutable string buffer with the specified capacity.
    /// </summary>
    /// <param name="capacity">The capacity</param>
    /// <param name="clearBuffer">Whether clearing the buffer. Has a slight performance hit</param>
    internal StringBuffer(int capacity, bool clearBuffer = false) {
        Length = capacity;
        _source = ArrayPool<char>.Shared.Rent(Length);
        if (clearBuffer) {
            Array.Clear(_source);
        }
        _buffer = _source.AsSpan(0, Length);
        _position = 0;
    }

    /// <summary>
    /// Creates a mutable string buffer of length 0 (empty)
    /// </summary>
    /// <remarks>
    /// It will throw if you try to append anything to it. use <see cref="StringBuffer(int, bool)"/> instead.
    /// </remarks>
    public StringBuffer() : this(0, false) { }

#pragma warning disable CS9084 // Struct member returns 'this' or other instance members by reference

    /// <summary>
    /// Appends a character to the string buffer.
    /// </summary>
    /// <param name="c">The character to append.</param>
    public ref StringBuffer Append(char c) {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(_position + 1, Length);
        _buffer[_position++] = c;
        return ref this;
    }

    /// <summary>
    /// Appends the specified string to the buffer.
    /// </summary>
    /// <param name="str">The string to append.</param>
    /// <returns>The same instance of the buffer</returns>
    public ref StringBuffer Append(scoped ReadOnlySpan<char> str) {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(_position + str.Length, Length);
        str.CopyTo(_buffer.Slice(_position));
        _position += str.Length;
        return ref this;
    }

    /// <summary>
    /// Appends a value to the string buffer, using the specified format and format provider.
    /// </summary>
    /// <typeparam name="T">The type of the value to append.</typeparam>
    /// <param name="value">The value to append.</param>
    /// <param name="format">The format specifier to apply to the value.</param>
    /// <param name="provider">The format provider to use.</param>
    /// <exception cref="InvalidOperationException">Thrown when the buffer is full.</exception>
    public ref StringBuffer Append<T>(T value, scoped ReadOnlySpan<char> format = default, IFormatProvider? provider = null) where T : ISpanFormattable {
        var appended = value.TryFormat(_buffer.Slice(_position), out var charsWritten, format, provider);
        if (!appended) {
            throw new ArgumentOutOfRangeException(nameof(Length));
        }
        _position += charsWritten;
        return ref this;
    }

    /// <summary>
    /// Appends the platform specific new line to the buffer.
    /// </summary>
    public ref StringBuffer AppendLine() {
        Append(NewLine);
        return ref this;
    }

    /// <summary>
    /// Appends the specified character to the buffer followed by the platform specific new line.
    /// </summary>
    /// <param name="c"></param>
    public ref StringBuffer AppendLine(char c) {
        Append(c);
        Append(NewLine);
        return ref this;
    }

    /// <summary>
    /// Appends the specified string to the buffer followed by the platform specific new line.
    /// </summary>
    /// <param name="str">The string to append.</param>
    /// <returns>The same instance of the buffer</returns>
    public ref StringBuffer AppendLine(scoped ReadOnlySpan<char> str) {
        Append(str);
        Append(NewLine);
        return ref this;
    }

    /// <summary>
    /// Appends a value to the string buffer, using the specified format and format provider, followed by the platform specific new line.
    /// </summary>
    /// <typeparam name="T">The type of the value to append.</typeparam>
    /// <param name="value">The value to append.</param>
    /// <param name="format">The format specifier to apply to the value.</param>
    /// <param name="provider">The format provider to use.</param>
    /// <exception cref="InvalidOperationException">Thrown when the buffer is full.</exception>
    public ref StringBuffer AppendLine<T>(T value, scoped ReadOnlySpan<char> format = default, IFormatProvider? provider = null) where T : ISpanFormattable {
        Append(value, format, provider);
        Append(NewLine);
        return ref this;
    }

#pragma warning restore CS9084 // Struct member returns 'this' or other instance members by reference

    /// <summary>
    /// Allocates a string from the internal buffer.
    /// </summary>
    /// <param name="trimIfShorter">Indicates whether to trim the string from the end.</param>
    /// <param name="trimEndWhiteSpace">Will try to trim the end white spaces</param>
    /// <returns>The allocated string.</returns>
    public readonly string Allocate(bool trimIfShorter = true, bool trimEndWhiteSpace = false) {
        ReadOnlySpan<char> span = _buffer;
        if (trimIfShorter) {
            span = span.Slice(0, _position);
        }
        if (trimEndWhiteSpace) {
            span = span.TrimEnd();
        }
        return new string(span);
    }

    /// <summary>
    /// Returns the character at the specified index.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public readonly char this[int index] => _buffer[index];

    /// <summary>
    /// Returns the used portion of the buffer as a readonly span.
    /// </summary>
    public readonly ReadOnlySpan<char> WrittenSpan => _buffer.Slice(0, _position);

    /// <summary>
    /// Allocates a substring from the internal buffer using the specified range.
    /// </summary>
    /// <param name="range"></param>
    private readonly string Allocate(Range range) {
        (int offset, int length) = range.GetOffsetAndLength(Length);
        ReadOnlySpan<char> span = _buffer.Slice(offset, length);
        return new string(span);
    }

    /// <summary>
    /// Returns a readonly span of the internal buffer up to the index after the last appended item.
    /// </summary>
    /// <param name="buffer"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator ReadOnlySpan<char>(StringBuffer buffer) => buffer.WrittenSpan;

    /// <summary>
    /// Returns a readonly memory of the internal buffer up to the index after the last appended item.
    /// </summary>
    /// <param name="buffer"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator ReadOnlyMemory<char>(StringBuffer buffer) => new(buffer._source, 0, buffer._position);

    /// <summary>
    /// Returns a string allocated from the StringBuffer.
    /// </summary>
    /// <remarks>It is identical to <see cref="Allocate(bool, bool)"/></remarks>
    public override readonly string ToString() => Allocate(true, false);

    /// <summary>
    /// Releases the resources used by the StringBuffer.
    /// </summary>
    public void Dispose() {
        if (_disposed) {
            return;
        }
        ArrayPool<char>.Shared.Return(_source, false);
        _disposed = true;
    }
}