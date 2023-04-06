using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Sharpify;

public static partial class Extensions {
    /// <summary>
    /// A simple wrapper over <see cref="string.IsNullOrEmpty(string)"/> to make it easier to use.
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static bool IsNullOrEmpty(this string str) => string.IsNullOrEmpty(str);

    /// <summary>
    /// A simple wrapper over <see cref="string.IsNullOrWhiteSpace(string)"/> to make it easier to use.
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static bool IsNullOrWhiteSpace(this string str) => string.IsNullOrWhiteSpace(str);

    /// <summary>
    /// Converts a string to an int32.
    /// </summary>
    /// <param name="value"></param>
    public static int ConvertToInt32(this string value) {
        if (string.IsNullOrWhiteSpace(value)) {
            return 0;
        }
        var str = value.AsSpan();
        bool isNegative = str[0] is '-';
        if (isNegative) {
            str = str[1..];
        }
        var num = 0;
        for (var i = 0; i < str.Length; i++) {
            var digit = str[i] - '0';

            // Check for invalid digit
            if (digit is < 0 or > 9) {
                return 0;
            }

            // Check for overflow
            if (num > (int.MaxValue - digit) / 10) {
                return 0;
            }

            num = (num * 10) + digit;
        }
        return isNegative ? -1 * num : num;
    }

    /// <summary>
    /// Converts a <see cref="ReadOnlySpan{T}"/> where T is <see langword="char"/> to an int32 (Use when you are sure it will only be positive).
    /// </summary>
    /// <param name="str"></param>
    /// <remarks>
    /// Here, invalid returns -1, you could use this to check if the conversion was successful.
    /// </remarks>
    public static int ConvertsToInt32Unsigned(this ReadOnlySpan<char> str) {
        var num = 0;
        ConvertToInt32Unsigned(str, ref num);
        return num;
    }

    /// <summary>
    /// Converts a string or <see cref="ReadOnlySpan{T}"/> into an int32 ref.
    /// </summary>
    /// <param name="str"></param>
    /// <param name="num"></param>
    public static void ConvertToInt32Unsigned(this ReadOnlySpan<char> str, ref int num) {
        // Check for empty string or larger than Int.MaxValue
        if (str.IsEmpty) {
            num = -1;
            return;
        }
        for (var i = 0; i < str.Length; i++) {
            var digit = str[i] - '0';

            // Check for invalid digit
            if (digit is < 0 or > 9) {
                num = -1;
                break;
            }

            // Check for overflow
            if (num > (int.MaxValue - digit) / 10) {
                num = -1;
                return;
            }

            num = (num * 10) + digit;
        }
    }

    /// <summary>
    /// Converts a string to an int32 (Use when you are sure it will only be positive).
    /// </summary>
    /// <param name="str"></param>
    /// <remarks>
    /// Here, invalid returns -1, you could use this to check if the conversion was successful.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ConvertToInt32Unsigned(this string str) => str.AsSpan().ConvertsToInt32Unsigned();

    /// <summary>
    /// Tries to convert a string to an int32.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="num"></param>
    /// <remarks>
    /// In case it fails, the value of num is not changed.
    /// </remarks>
    public static bool TryConvertToInt32(this string value, ref int num) {
        // Check for empty string or larger than Int.MaxValue
        if (value.Length is 0 or > 10) {
            return false;
        }
        var str = value.AsSpan();
        int prevValue = num;
        num = 0;
        for (var i = 0; i < str.Length; i++) {
            var digit = str[i] - '0';

            // Check for invalid digit
            if (digit is < 0 or > 9) {
                num = prevValue;
                return false;
            }

            // Check for overflow
            if (num > (int.MaxValue - digit) / 10) {
                num = prevValue;
                return false;
            }

            num = (num * 10) + digit;
        }
        return true;
    }

    /// <summary>
    /// Suffixes a string with a <see cref="ReadOnlySpan{T}"/> where T is <see langword="char"/>.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="suffix"></param>
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
            return new string(value);
        }
        var str = value.AsSpan();
        Span<char> res = stackalloc char[str.Length + suffix.Length];
        Span<char> resSpan = res;
        str.CopyTo(resSpan);
        resSpan = resSpan[str.Length..];
        suffix.CopyTo(resSpan);
        return new string(res);
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
    /// <param name="str"></param>
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