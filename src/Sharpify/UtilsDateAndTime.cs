using System.Buffers;
using System.Globalization;

using Sharpify.Collections;

namespace Sharpify;

public static partial class Utils {
    /// <summary>
    /// Provides utility methods for <see cref="DateTime"/>
    /// </summary>
    public static class DateAndTime {
        /// <summary>
        /// Returns a <see cref="ValueTask"/> of the current time
        /// </summary>
        /// <remarks>
        /// This is useful for firing off this task and awaiting it later, because <see cref="DateTime.Now"/> actually takes quite a bit of time
        /// </remarks>
        public static ValueTask<DateTime> GetCurrentTimeAsync() => ValueTask.FromResult(DateTime.Now);

        /// <summary>
        /// Returns a <see cref="ValueTask"/> of the current time in binary
        /// </summary>
        /// <remarks>
        /// This is useful for firing off this task and awaiting it later, because <see cref="DateTime.Now"/> actually takes quite a bit of time
        /// </remarks>
        public static ValueTask<long> GetCurrentTimeInBinaryAsync() => ValueTask.FromResult(DateTime.Now.ToBinary());

        private const int TimeSpanRequiredBufferLength = 30;

        /// <summary>
        /// Formats a <see cref="TimeSpan"/> to a pretty string, with 2 sections, e.g., "12:34hr" or "05:12d" or "500ms" etc...
        /// </summary>
        /// <param name="timeSpan">The TimeSpan to format</param>
        /// <param name="buffer">The Buffer to use</param>
        /// <remarks>
        /// Ensure capacity >= 30
        /// </remarks>
        /// <returns><see cref="ReadOnlySpan{Char}"/> part of the written buffer</returns>
        public static ReadOnlySpan<char> FormatTimeSpan(TimeSpan timeSpan, Span<char> buffer) {
            var sb = StringBuffer.Create(buffer);
            switch (timeSpan.TotalSeconds) {
                case < 1: // Milliseconds: e.g., "500ms"
                    sb.Append(timeSpan.Milliseconds);
                    sb.Append("ms");
                    break;
                case < 60:
                    // Seconds:Milliseconds: e.g., "03:05s"
                    if (timeSpan.Seconds < 10) {
                        sb.Append('0');
                    }
                    sb.Append(timeSpan.Seconds);
                    sb.Append(':');
                    if (timeSpan.Milliseconds < 100) {
                        sb.Append('0');
                        if (timeSpan.Milliseconds < 10) {
                            sb.Append('0');
                        }
                    }
                    sb.Append(timeSpan.Milliseconds);
                    sb.Append("s");
                    break;
                case < 3600:
                    // Minutes:Seconds: e.g., "01:30m"
                    if (timeSpan.Minutes < 10) {
                        sb.Append('0');
                    }
                    sb.Append(timeSpan.Minutes);
                    sb.Append(':');
                    if (timeSpan.Seconds < 10) {
                        sb.Append('0');
                    }
                    sb.Append(timeSpan.Seconds);
                    sb.Append("m");
                    break;
                case < 86400:
                    // Hours:Minutes e.g., "12:34hr"
                    if (timeSpan.Hours < 10) {
                        sb.Append('0');
                    }
                    sb.Append(timeSpan.Hours);
                    sb.Append(':');
                    if (timeSpan.Minutes < 10) {
                        sb.Append('0');
                    }
                    sb.Append(timeSpan.Minutes);
                    sb.Append("hr");
                    break;
                default:
                    // Days:Hours e.g., "05:12d"
                    if (timeSpan.Days < 10) {
                        sb.Append('0');
                    }
                    sb.Append(timeSpan.Days);
                    sb.Append(':');
                    if (timeSpan.Hours < 10) {
                        sb.Append('0');
                    }
                    sb.Append(timeSpan.Hours);
                    sb.Append("d");
                    break;
            }

            return sb.WrittenSpan;
        }

        /// <summary>
        /// Formats a <see cref="TimeSpan"/> to a pretty string, with 2 sections, e.g., "12:34hr" or "05:12d" or "500ms" etc...
        /// </summary>
        /// <param name="timeSpan">The TimeSpan to format</param>
        /// <returns>a string representing the TimeSpan</returns>
        public static string FormatTimeSpan(TimeSpan timeSpan) {
            using var owner = MemoryPool<char>.Shared.Rent(TimeSpanRequiredBufferLength);
            return new string(FormatTimeSpan(timeSpan, owner.Memory.Span));
        }

        /// <summary>
        /// Returns a Time Stamp (HHMM-dd-mmm-yy) formatted into an existing buffer
        /// </summary>
        /// <returns><see cref="StringBuffer"/></returns>
        /// <remarks>
        /// Ensure capacity >= 30
        /// </remarks>
        public static ReadOnlySpan<char> FormatTimeStamp(DateTime time, Span<char> buffer) {
            return StringBuffer.Create(buffer)
                               .Append(time.Hour)
                               .Append(time.Minute)
                               .Append('-')
                               .Append(time.Day)
                               .Append('-')
                               .Append(CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(time.Month))
                               .Append('-')
                               .Append(time.Year % 100)
                               .WrittenSpan;
        }

        /// <summary>
        /// Returns a Time Stamp -> HHMM-dd-mmm-yy
        /// </summary>
        public static string FormatTimeStamp(DateTime time) {
            using var owner = MemoryPool<char>.Shared.Rent(TimeSpanRequiredBufferLength);
            return new string(FormatTimeStamp(time, owner.Memory.Span));
        }
    }
}