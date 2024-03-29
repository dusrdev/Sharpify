using System.Runtime.CompilerServices;

using Sharpify.Collections;

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

            var buffer = StringBuffer.Create(stackalloc char[10]);
            if (bytes < kb) {
                buffer.Append(Math.Round(bytes, 2));
                buffer.Append(' ');
                buffer.Append(FileSizeSuffix[0]);
                return buffer.Allocate(true);
            }
            var suffix = 0;
            while (bytes >= kb && suffix < FileSizeSuffix.Length) {
                bytes *= divisor;
                suffix++;
            }
            buffer.Append(Math.Round(bytes, 2));
            buffer.Append(' ');
            buffer.Append(FileSizeSuffix[suffix]);
            return buffer.Allocate(true);
        }
    }
}