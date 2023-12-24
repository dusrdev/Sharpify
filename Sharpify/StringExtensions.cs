using System.Buffers;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Sharpify;

public static partial class Extensions {
    /// <summary>
    /// Gets a reference to the first character of the string.
    /// </summary>
    /// <param name="text">The string.</param>
    /// <returns>A reference to the first character of the string.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref char GetReference(this string text) {
        return ref Unsafe.AsRef(in text.GetPinnableReference());
    }

    /// <summary>
    /// A simple wrapper over <see cref="string.IsNullOrEmpty(string)"/> to make it easier to use.
    /// </summary>
    public static bool IsNullOrEmpty(this string str) => string.IsNullOrEmpty(str);

    /// <summary>
    /// A simple wrapper over <see cref="string.IsNullOrWhiteSpace(string)"/> to make it easier to use.
    /// </summary>
    public static bool IsNullOrWhiteSpace(this string str) => string.IsNullOrWhiteSpace(str);


#pragma warning disable CS1584 // XML comment has syntactically incorrect cref attribute


#pragma warning disable CS1658 // Warning is overriding an error

    /// <summary>
    /// Tries to convert a <see cref="ReadOnlySpan{char}"/> to an <see cref="int"/>.
    /// </summary>
    /// <param name="value">The <see cref="ReadOnlySpan{char}"/> to convert.</param>
    /// <param name="result">When this method returns, contains the converted <see cref="int"/> if the conversion succeeded, or zero if the conversion failed.</param>
    /// <returns><c>true</c> if the conversion succeeded; otherwise, <c>false</c>.</returns>
    public static bool TryConvertToInt32(this ReadOnlySpan<char> value, out int result) {
        result = 0;
        if (value.IsWhiteSpace() || value.Length > 11) { // 10 is the max length of an int32 + 1 for sign
            return false;
        }
        bool isNegative = value[0] is '-';
        var length = value.Length;
        int i = 0;
        if (isNegative) {
            i++;
        }
        for (; i < length; i++) {
            var digit = value[i] - '0';

            // Check for invalid digit
            if (digit is < 0 or > 9) {
                result = 0;
                return false;
            }

            unchecked {
                result = (result * 10) + digit;
            }
        }
        if (isNegative) {
            result *= -1;
        }
        return true;
    }
#pragma warning restore CS1658 // Warning is overriding an error
#pragma warning restore CS1584 // XML comment has syntactically incorrect cref attribute

    /// <summary>
    /// Suffixes a string with a <see cref="ReadOnlySpan{T}"/> where T is <see langword="char"/>.
    /// </summary>
    /// <param name="value">The first portion</param>
    /// <param name="suffix">The second portion</param>
    /// <remarks>
    /// <para>
    /// This method is slightly slower than <see cref="string.Concat(ReadOnlySpan{char}, ReadOnlySpan{char})"/>
    /// Or string interpolation but uses half the memory.
    /// </para>
    /// <para>This advantage diminishes when more than 2 strings are used.</para>
    /// </remarks>
    public static string Suffix(this string value, ReadOnlySpan<char> suffix) {
        if (value.Length is 0 && suffix.Length is 0) {
            return string.Empty;
        }
        if (value.Length is 0) {
            return new string(suffix);
        }
        if (suffix.Length is 0) {
            return value;
        }
        var str = value.AsSpan();
        var length = str.Length + suffix.Length;
        char[] arr = ArrayPool<char>.Shared.Rent(length);
        Span<char> resSpan = arr;
        str.CopyTo(resSpan);
        suffix.CopyTo(resSpan[str.Length..]);
        var res = new string(arr[0..length]);
        ArrayPool<char>.Shared.Return(arr);
        return res;
    }

    /// <summary>
    /// A more convenient way to use <see cref="string.Concat(ReadOnlySpan{char}, ReadOnlySpan{char})"/>
    /// </summary>
    /// <param name="value"></param>
    /// <param name="suffix"></param>
    /// <remarks>
    /// The advantage of Concat over string interpolation diminishes when more than 2 strings are used.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string Concat(this string value, ReadOnlySpan<char> suffix) => string.Concat(value.AsSpan(), suffix);

    /// <summary>
    /// Method used to turn <paramref name="str"/> into Title format
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToTitle(this string str) => CultureInfo.CurrentCulture.TextInfo.ToTitleCase(str);

    /// <summary>
    /// Checks if a string is a valid binary string (0,1,' ','\t','\n','\r')
    /// </summary>
    public static bool IsBinary(this string str) {
        foreach (var c in str.AsSpan()) {
            if (c is '0' or '1' || char.IsWhiteSpace(c)) {
                continue;
            }
            return false;
        }
        return true;
    }
}