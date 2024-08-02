using Sharpify.Collections;

namespace Sharpify;

public static partial class Utils {
    /// <summary>
    /// Provides utility methods for <see cref="string"/>
    /// </summary>
    public static class Strings {
        private static ReadOnlySpan<string> FileSizeSuffix => new string[] { "B", "KB", "MB", "GB", "TB", "PB" };
        private const int FormatBytesRequiredLength = 10;
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
            var suffix = 0;
            while (bytes >= FormatBytesKb && suffix < FileSizeSuffix.Length) {
                bytes *= FormatBytesDivisor;
                suffix++;
            }
            return StringBuffer.Create(stackalloc char[FormatBytesRequiredLength])
                               .Append(Math.Round(bytes, 2))
                               .Append(' ')
                               .Append(FileSizeSuffix[suffix])
                               .Allocate(true, true);
        }

        /// <summary>
        /// Formats bytes to friendlier strings, i.e: B,KB,MB,TB,PB... into a rented buffer
        /// </summary>
        /// <returns><see cref="StringBuffer"/></returns>
        /// <remarks>
        /// Make sure to dispose the buffer after use (you can use it in a using statement), view the result with <see cref="StringBuffer.WrittenSpan"/>
        /// </remarks>
        public static StringBuffer FormatBytesInRentedBuffer(long bytes)
            => FormatBytesInRentedBuffer((double)bytes);

        /// <summary>
        /// Formats bytes to friendlier strings, i.e: B,KB,MB,TB,PB... into a rented buffer
        /// </summary>
        /// <returns><see cref="StringBuffer"/></returns>
        /// <remarks>
        /// Make sure to dispose the buffer after use (you can use it in a using statement), view the result with <see cref="StringBuffer.WrittenSpan"/>
        /// </remarks>
        public static StringBuffer FormatBytesInRentedBuffer(double bytes) {
            var suffix = 0;
            while (bytes >= FormatBytesKb && suffix < FileSizeSuffix.Length) {
                bytes *= FormatBytesDivisor;
                suffix++;
            }
            return StringBuffer.Rent(FormatBytesRequiredLength)
                               .Append(Math.Round(bytes, 2))
                               .Append(' ')
                               .Append(FileSizeSuffix[suffix]);
        }
    }
}