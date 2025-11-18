using System.Buffers; // required for SearchValues<T>
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
    /// Method used to turn <paramref name="str"/> into Title format
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToTitle(this string str) => CultureInfo.CurrentCulture.TextInfo.ToTitleCase(str);

    private const string BinaryChars = "01 \t\n\r";
    private static readonly SearchValues<char> BinarySearchValues = SearchValues.Create(BinaryChars);

    /// <summary>
    /// Checks if a string is a valid binary string (0,1,' ','\t','\n','\r')
    /// </summary>
    public static bool IsBinary(this string str) {
        return !str.AsSpan().ContainsAnyExcept(BinarySearchValues);
    }
}