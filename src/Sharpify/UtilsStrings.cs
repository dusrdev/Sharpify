using System.Numerics;

namespace Sharpify;

public static partial class Utils {
    private static readonly string[] FileSizeSuffix = ["B", "KB", "MB", "GB", "TB", "PB"];

    private static string GetSuffix(double bytes) {
        const double formatBytesKb = 1024d;
        const double formatBytesDivisor = 1 / formatBytesKb;
        var suffix = 0;
        while (suffix < FileSizeSuffix.Length - 1 && bytes >= formatBytesKb) {
            bytes *= formatBytesDivisor;
            suffix++;
        }
        return FileSizeSuffix[suffix];
    }

    /// <summary>
    /// Returns <paramref name="bytes"/> formatted in a human readable way.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="bytes"></param>
    /// <returns></returns>
    public static string FormatBytes<T>(T bytes) where T : INumberBase<T> {
        var b = double.CreateChecked(bytes);
        var suffix = GetSuffix(b);
        return $"{bytes:#,##0.##} {suffix}";
    }

    /// <summary>
    /// Returns a <see cref="ReadOnlySpan{Char}"/> slice over <paramref name="bytes"/> formatted in a human readable way.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="bytes"></param>
    /// <param name="buffer">Ensure buffer is big enough, 512 length is sufficient to cover <see cref="double.MaxValue"/> bytes, but this number is theoretically higher than any real world case. Test if want a smaller buffer.</param>
    /// <returns></returns>
    public static ReadOnlySpan<char> FormatBytes<T>(T bytes, Span<char> buffer) where T : INumberBase<T> {
        var b = double.CreateChecked(bytes);
        var suffix = GetSuffix(b);
        if (!buffer.TryWrite($"{bytes:#,##0.##} {suffix}", out int written)) {
            return ReadOnlySpan<char>.Empty;
        }
        return buffer.Slice(0, written);
    }
}