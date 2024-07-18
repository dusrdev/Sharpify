namespace Sharpify.Collections;

/// <summary>
/// Represents a mutable string buffer that allows efficient appending of characters, strings and other <see cref="ISpanFormattable"/> implementations.
/// </summary>
public unsafe ref partial struct StringBuffer {
    /// <summary>
    /// Initializes a string buffer that uses memory rented from the array pool.
    /// </summary>
    public static StringBuffer Rent(int capacity, bool clearBuffer = false) => new(capacity, clearBuffer);

    /// <summary>
    /// Initializes a string buffer that uses a pre-allocated buffer (potentially from the stack).
    /// </summary>
    public static AllocatedStringBuffer Create(Span<char> buffer) => new(buffer);

    /// <summary>
    /// Initializes a string buffer that uses a pre-allocated array buffer (not stack).
    /// </summary>
    public static AllocatedStringBuffer Create(char[] buffer) => new(buffer);
}