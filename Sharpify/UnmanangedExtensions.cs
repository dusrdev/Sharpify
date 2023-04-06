using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Text;

namespace Sharpify;

public static partial class Extensions {
    /// <summary>
    /// Returns a rolling average
    /// </summary>
    /// <param name="val"></param>
    /// <param name="newVal"></param>
    /// <param name="count"></param>
    public static double RollingAverage(
        this double val,
        double newVal,
        int count) =>
                  count is 0
                  ? newVal
                  : (val * (count - 1) / count) + (newVal / count);

    private static readonly ImmutableArray<string> FileSizeSuffix =
        ImmutableArray.Create("B", "KB", "MB", "GB", "TB", "PB");

    private static readonly ThreadLocal<StringBuilder> ByteFormattingBuilder = new(static () => new StringBuilder());

    /// <summary>
    /// Formats bytes to friendlier strings, i.e: B,KB,MB,TB,PB...
    /// </summary>
    /// <param name="bytes"></param>
    /// <returns>string</returns>
    public static string FormatBytes(this long bytes) => ((double)bytes).FormatBytes();

    /// <summary>
    /// Formats bytes to friendlier strings, i.e: B,KB,MB,TB,PB...
    /// </summary>
    /// <param name="bytes"></param>
    /// <returns>string</returns>
    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.NoInlining)]
    public static string FormatBytes(this double bytes) {
        var byteFormattingBuilder = ByteFormattingBuilder.Value;
        byteFormattingBuilder!.Clear();
        var suffix = 0;
        while (bytes >= 1024 && suffix < FileSizeSuffix.Length) {
            bytes /= 1024;
            suffix++;
        }
        return byteFormattingBuilder.Append(Math.Round(bytes, 2))
                                     .Append(' ')
                                     .Append(FileSizeSuffix[suffix])
                                     .ToString();
    }

    /// <summary>
    /// Tries to parse an enum result from a string
    /// </summary>
    /// <typeparam name="TEnum"></typeparam>
    /// <param name="value"></param>
    /// <param name="result"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryParseAsEnum<TEnum>(
        this string value,
        out TEnum result) where TEnum : struct, Enum {
        return Enum.TryParse(value, out result);
    }
}