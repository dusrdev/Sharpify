using System.Runtime.CompilerServices;

namespace Sharpify;

public static partial class Extensions {
    private static readonly ReadOnlyMemory<string> FileSizeSuffix = new(["B", "KB", "MB", "GB", "TB", "PB"]);

    /// <summary>
    /// Formats bytes to friendlier strings, i.e: B,KB,MB,TB,PB...
    /// </summary>
    /// <returns>string</returns>
    public static string FormatBytes(this long bytes) => ((double)bytes).FormatBytes();

    /// <summary>
    /// Formats bytes to friendlier strings, i.e: B,KB,MB,TB,PB...
    /// </summary>
    /// <returns>string</returns>
    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.NoInlining)]
    public static string FormatBytes(this double bytes) {
        const double kb = 1024d;
        const double divisor = 1 / kb;
        var suffix = 0;
        while (bytes >= kb && suffix < FileSizeSuffix.Length) {
            bytes *= divisor;
            suffix++;
        }
        Span<char> buffer = stackalloc char[9];
        Math.Round(bytes, 2).TryFormat(buffer, out var charsWritten);
        buffer[charsWritten++] = ' ';
        FileSizeSuffix.Span[suffix].CopyTo(buffer[charsWritten..]);
        int len = suffix is 0 ? charsWritten + 1 : charsWritten + 2;
        return new string(buffer[0..len]);
    }

    /// <summary>
    /// Tries to parse an enum result from a string
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryParseAsEnum<TEnum>(
        this string value,
        out TEnum result) where TEnum : struct, Enum {
        return Enum.TryParse(value, out result) && Enum.IsDefined(result);
    }
}