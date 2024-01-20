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

    /// <summary>
    /// Tries to convert <paramref name="value"/> to an <see cref="int"/>.
    /// </summary>
    /// <param name="value">The span of characters to convert.</param>
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

#if NET8_0_OR_GREATER
    private const string BinaryChars = "01 \t\n\r";
    private static readonly SearchValues<char> BinarySearchValues = SearchValues.Create(BinaryChars);

    /// <summary>
    /// Checks if a string is a valid binary string (0,1,' ','\t','\n','\r')
    /// </summary>
    public static bool IsBinary(this string str) {
        return !str.AsSpan().ContainsAnyExcept(BinarySearchValues);
    }
#elif NET7_0_OR_GREATER
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
#endif
}