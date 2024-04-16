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
        public static string FormatBytes(long bytes)
            => new(FormatBytesNonAllocated(bytes, stackalloc char[10]));

        /// <summary>
        /// Formats bytes to friendlier strings, i.e: B,KB,MB,TB,PB...
        /// </summary>
        /// <returns>string</returns>
        /// <remarks>
        /// <para>
        /// This version does not allocate the string, and reuses an existing buffer
        /// </para>
        /// <para>
        /// Make sure the buffer is at least 10 characters long to be safe
        /// </para>
        /// </remarks>
        public static ReadOnlySpan<char> FormatBytesNonAllocated(long bytes, Span<char> buffer)
            => FormatBytesNonAllocated(bytes, Array.Empty<char>(), buffer);

        /// <summary>
        /// Formats bytes to friendlier strings, i.e: B,KB,MB,TB,PB...
        /// </summary>
        /// <returns>string</returns>
        /// <remarks>
        /// <para>
        /// This version does not allocate the string, and reuses an existing buffer
        /// </para>
        /// <para>
        /// Make sure the buffer is at least 10 characters long to be safe
        /// </para>
        /// </remarks>
        public static ReadOnlyMemory<char> FormatBytesNonAllocated(long bytes, char[] buffer)
            => FormatBytesNonAllocated(bytes, buffer, Span<char>.Empty);

        /// <summary>
        /// Formats bytes to friendlier strings, i.e: B,KB,MB,TB,PB...
        /// </summary>
        /// <returns>string</returns>
        public static string FormatBytes(double bytes)
            => new(FormatBytesNonAllocated(bytes, stackalloc char[10]));

        /// <summary>
        /// Formats bytes to friendlier strings, i.e: B,KB,MB,TB,PB...
        /// </summary>
        /// <returns>string</returns>
        /// <remarks>
        /// <para>
        /// This version does not allocate the string, and reuses an existing buffer
        /// </para>
        /// <para>
        /// Make sure the buffer is at least 10 characters long to be safe
        /// </para>
        /// </remarks>
        public static ReadOnlySpan<char> FormatBytesNonAllocated(double bytes, Span<char> buffer)
            => FormatBytesNonAllocated(bytes, Array.Empty<char>(), buffer);

        /// <summary>
        /// Formats bytes to friendlier strings, i.e: B,KB,MB,TB,PB...
        /// </summary>
        /// <returns>string</returns>
        /// <remarks>
        /// <para>
        /// This version does not allocate the string, and reuses an existing buffer
        /// </para>
        /// <para>
        /// Make sure the buffer is at least 10 characters long to be safe
        /// </para>
        /// </remarks>
        public static ReadOnlyMemory<char> FormatBytesNonAllocated(double bytes, char[] buffer)
            => FormatBytesNonAllocated(bytes, buffer, Span<char>.Empty);

        /// <summary>
        /// Formats bytes to friendlier strings, i.e: B,KB,MB,TB,PB...
        /// </summary>
        /// <returns>string</returns>
        /// <remarks>
        /// <para>
        /// This version does not allocate the string, and reuses an existing buffer
        /// </para>
        /// If the buffer is empty, the span is used, and the other way around.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.NoInlining)]
        public static AllocatedStringBuffer FormatBytesNonAllocated(double bytes, Span<char> buffer, Span<char> sBuffer) {
            const double kb = 1024d;
            const double divisor = 1 / kb;

            var buf = buffer.Length is 0
                ? StringBuffer.Create(sBuffer)
                : StringBuffer.Create(buffer!);
            if (bytes < kb) {
                buf.Append(Math.Round(bytes, 2));
                buf.Append(' ');
                buf.Append(FileSizeSuffix[0]);
                return buf;
            }
            var suffix = 0;
            while (bytes >= kb && suffix < FileSizeSuffix.Length) {
                bytes *= divisor;
                suffix++;
            }
            buf.Append(Math.Round(bytes, 2));
            buf.Append(' ');
            buf.Append(FileSizeSuffix[suffix]);
            return buf;
        }
    }
}