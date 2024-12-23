using System.Buffers;

using Sharpify.Collections;

namespace Sharpify;

public static partial class Utils {
    /// <summary>
    /// Provides utility methods for <see cref="string"/>
    /// </summary>
    public static class Strings {
        private static ReadOnlySpan<string> FileSizeSuffix => new string[] { "B", "KB", "MB", "GB", "TB", "PB" };

        /// <summary>
        /// The required length of the buffer to format bytes
        /// </summary>
        /// <remarks>
        /// This is required to handle <see cref="double.MaxValue"/>, usual cases are much smaller, and this internal uses are pooled.
        /// </remarks>
        public const int FormatBytesRequiredLength = 512;
        private const double FormatBytesKb = 1024d;
        private const double FormatBytesDivisor = 1 / FormatBytesKb;

        /// <summary>
        /// Formats bytes to friendlier strings, i.e: B,KB,MB,TB,PB...
        /// </summary>
        /// <returns>string</returns>
        public static string FormatBytes(long bytes)
            => FormatBytes((double)bytes);

        /// <summary>
        /// Formats bytes to friendlier strings, i.e: B,KB,MB,TB,PB...
        /// </summary>
        /// <returns>string</returns>
        public static string FormatBytes(double bytes) {
            using var owner = MemoryPool<char>.Shared.Rent(FormatBytesRequiredLength);
            return new string(FormatBytes(bytes, owner.Memory.Span));
        }

        /// <summary>
        /// Formats bytes to friendlier strings, i.e: B,KB,MB,TB,PB... into the buffer and returns the written span
        /// </summary>
        /// <remarks>
        /// Ensure capacity >= <see cref="FormatBytesRequiredLength"/>
        /// </remarks>
        /// <returns>string</returns>
        public static ReadOnlySpan<char> FormatBytes(double bytes, Span<char> buffer) {
            var suffix = 0;
            while (suffix < FileSizeSuffix.Length - 1 && bytes >= FormatBytesKb) {
                bytes *= FormatBytesDivisor;
                suffix++;
            }
            return StringBuffer.Create(buffer)
                               .Append(bytes, "#,##0.##")
                               .Append(' ')
                               .Append(FileSizeSuffix[suffix])
                               .Allocate();
        }

        /// <summary>
        /// Formats bytes to friendlier strings, i.e: B,KB,MB,TB,PB... into the buffer and returns the written span
        /// </summary>
        /// <remarks>
        /// Ensure capacity >= <see cref="FormatBytesRequiredLength"/>
        /// </remarks>
        /// <returns>string</returns>
        public static ReadOnlySpan<char> FormatBytes(long bytes, Span<char> buffer)
            => FormatBytes((double)bytes, buffer);
    }
}