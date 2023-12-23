using System.Buffers;
using System.Runtime.CompilerServices;

namespace Sharpify;

public static partial class Utils {
    /// <summary>
    /// Provides utility methods for <see cref="string"/>
    /// </summary>
    public static class Strings {
        private static ReadOnlySpan<string> FileSizeSuffix => new string[] { "B", "KB", "MB", "GB", "TB", "PB" };

        /// <summary>
        /// Formats bytes to friendlier strings, i.e: B,KB,MB,TB,PB...
        /// </summary>
        /// <returns>string</returns>
        public static string FormatBytes(long bytes) => FormatBytes((double)bytes);

        /// <summary>
        /// Formats bytes to friendlier strings, i.e: B,KB,MB,TB,PB...
        /// </summary>
        /// <returns>string</returns>
        [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.NoInlining)]
        public static string FormatBytes(double bytes) {
            const double kb = 1024d;
            const double divisor = 1 / kb;
            var suffix = 0;
            while (bytes >= kb && suffix < FileSizeSuffix.Length) {
                bytes *= divisor;
                suffix++;
            }
            var arr = ArrayPool<char>.Shared.Rent(9);
            Span<char> buffer = arr;
            int index = 0;
            Math.Round(bytes, 2).TryFormat(buffer, out var charsWritten);
            index += charsWritten;
            buffer[index++] = ' ';
            var suffixString = FileSizeSuffix[suffix];
            suffixString.CopyTo(buffer[index..]);
            index += suffixString.Length;
            var res = new string(buffer[0..index]);
            ArrayPool<char>.Shared.Return(arr);
            return res;
        }
    }
}