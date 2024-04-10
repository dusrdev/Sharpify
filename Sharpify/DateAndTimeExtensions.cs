using System.Globalization;

using Sharpify.Collections;

namespace Sharpify;

public static partial class Extensions {
    private static readonly char[] DateTimeBuffer = GC.AllocateUninitializedArray<char>(30);

    /// <summary>
    /// Formats a <see cref="TimeSpan"/> to a pretty string
    /// </summary>
    public static string Format(this TimeSpan elapsed) => new(FormatNonAllocated(elapsed));

    /// <summary>
    /// Formats a <see cref="TimeSpan"/> to a pretty string
    /// </summary>
    /// <remarks>
    /// This version does not allocate the string, and reuses an internal buffer
    /// </remarks>
    public static ReadOnlySpan<char> FormatNonAllocated(this TimeSpan elapsed) {
        lock (DateTimeBuffer) {
            (double value, string suffix) = elapsed.TotalSeconds switch {
                < 1 => (elapsed.TotalMilliseconds, "ms"),
                < 60 => (elapsed.TotalSeconds, "s"),
                < 3600 => (elapsed.TotalMinutes, "m"),
                < 86400 => (elapsed.TotalHours, "hr"),
                _ => (elapsed.TotalDays, "d")
            };
            // The longest possible number is going to be days, since it's the largest unit of time
            // 23 digits long is fully formatted 10^14 which is 2 magnitudes more than the amount of days since earth was formed
            // it is rather a safe bet that we wouldn't surpass it
            var buffer = StringBuffer.Create(DateTimeBuffer);
            buffer.Append(Math.Round(value, 2));
            buffer.Append(suffix);
            return DateTimeBuffer.AsSpan()[buffer.WrittenRange];
        }
    }

    /// <summary>
    /// Formats time span to human readable format
    /// </summary>
    public static string ToRemainingDuration(this TimeSpan time)
        => new(ToRemainingDurationNonAllocated(time));

    /// <summary>
    /// Formats time span to human readable format
    /// </summary>
    /// <remarks>
    /// This version does not allocate the string, and reuses an internal buffer
    /// </remarks>
    public static ReadOnlySpan<char> ToRemainingDurationNonAllocated(this TimeSpan time) {
        lock (DateTimeBuffer) {
            var buffer = StringBuffer.Create(DateTimeBuffer);

            if (time.TotalSeconds <= 1) {
                buffer.Append("0s");
                return DateTimeBuffer.AsSpan()[buffer.WrittenRange];
            }

            if (time.Days > 0) {
                buffer.Append(time.Days);
                buffer.Append("d ");
            }
            if (time.Hours > 0) {
                buffer.Append(time.Hours);
                buffer.Append("h ");
            }
            if (time.Minutes > 0) {
                buffer.Append(time.Minutes);
                buffer.Append("m ");
            }
            if (time.Seconds > 0) {
                buffer.Append(time.Seconds);
                buffer.Append("s ");
            }
            return DateTimeBuffer.AsSpan()[buffer.WrittenRange];
        }
    }

    /// <summary>
    /// Returns a Time Stamp -> HHMM-dd-mmm-yy
    /// </summary>
    public static string ToTimeStamp(this DateTime time)
        => new(ToTimeStampNonAllocated(time));

    /// <summary>
    /// Returns a Time Stamp -> HHMM-dd-mmm-yy
    /// </summary>
    /// <remarks>
    /// This version does not allocate the string, and reuses an internal buffer
    /// </remarks>
    public static ReadOnlySpan<char> ToTimeStampNonAllocated(this DateTime time) {
        lock (DateTimeBuffer) {
            var buffer = StringBuffer.Create(DateTimeBuffer);
            buffer.Append(time.Hour);
            buffer.Append(time.Minute);
            buffer.Append('-');
            buffer.Append(time.Day);
            buffer.Append('-');
            buffer.Append(CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(time.Month));
            buffer.Append('-');
            buffer.Append(time.Year % 100);
            return DateTimeBuffer.AsSpan()[buffer.WrittenRange];
        }
    }
}