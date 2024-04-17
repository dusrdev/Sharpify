using System.Globalization;

using Sharpify.Collections;

namespace Sharpify;

public static partial class Extensions {
    /// <summary>
    /// Formats a <see cref="TimeSpan"/> to a pretty string
    /// </summary>
    public static string Format(this TimeSpan elapsed)
        => FormatNonAllocated(elapsed, Array.Empty<char>(), stackalloc char[30]).Allocate(true, true);

    /// <summary>
    /// Formats a <see cref="TimeSpan"/> to a pretty string
    /// </summary>
    /// <remarks>
    /// <para>
    /// This version does not allocate the string, and reuses an existing buffer
    /// </para>
    /// <para>
    /// Make sure the buffer is at least 30 characters long to be safe
    /// </para>
    /// </remarks>
    public static ReadOnlySpan<char> FormatNonAllocated(this TimeSpan elapsed, Span<char> buffer)
        => FormatNonAllocated(elapsed, Array.Empty<char>(), buffer);

    /// <summary>
    /// Formats a <see cref="TimeSpan"/> to a pretty string
    /// </summary>
    /// <remarks>
    /// <para>
    /// This version does not allocate the string, and reuses an existing buffer
    /// </para>
    /// <para>
    /// Make sure the buffer is at least 30 characters long to be safe
    /// </para>
    /// </remarks>
    public static ReadOnlyMemory<char> FormatNonAllocated(this TimeSpan elapsed, char[] buffer)
        => FormatNonAllocated(elapsed, buffer, Span<char>.Empty);

    /// <summary>
    /// Formats a <see cref="TimeSpan"/> to a pretty string
    /// </summary>
    /// <remarks>
    /// <para>
    /// This version does not allocate the string, and reuses an existing buffer
    /// </para>
    /// If the buffer is empty, the span is used, and the other way around.
    /// </remarks>
    private static AllocatedStringBuffer FormatNonAllocated(this TimeSpan elapsed, char[] buffer, Span<char> sBuffer) {
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
        var buf = buffer.Length is 0
            ? StringBuffer.Create(sBuffer)
            : StringBuffer.Create(buffer!);

        buf.Append(Math.Round(value, 2));
        buf.Append(suffix);
        return buf;
    }

    /// <summary>
    /// Formats time span to human readable format
    /// </summary>
    public static string ToRemainingDuration(this TimeSpan time)
        => ToRemainingDurationNonAllocated(time, Array.Empty<char>(), stackalloc char[30]).Allocate(true, true);

    /// <summary>
    /// Formats time span to human readable format
    /// </summary>
    /// <remarks>
    /// <para>
    /// This version does not allocate the string, and reuses an existing buffer
    /// </para>
    /// <para>
    /// Make sure the buffer is at least 30 characters long to be safe
    /// </para>
    /// </remarks>
    public static ReadOnlySpan<char> ToRemainingDurationNonAllocated(this TimeSpan time, Span<char> buffer)
        => ToRemainingDurationNonAllocated(time, Array.Empty<char>(), buffer);

    /// <summary>
    /// Formats time span to human readable format
    /// </summary>
    /// <remarks>
    /// <para>
    /// This version does not allocate the string, and reuses an existing buffer
    /// </para>
    /// <para>
    /// Make sure the buffer is at least 30 characters long to be safe
    /// </para>
    /// </remarks>
    public static ReadOnlyMemory<char> ToRemainingDurationNonAllocated(this TimeSpan time, char[] buffer)
        => ToRemainingDurationNonAllocated(time, buffer, Span<char>.Empty);

    /// <summary>
    /// Formats time span to human readable format
    /// </summary>
    /// <remarks>
    /// <para>
    /// This version does not allocate the string, and reuses an existing buffer
    /// </para>
    /// If the buffer is empty, the span is used, and the other way around.
    /// </remarks>
    private static AllocatedStringBuffer ToRemainingDurationNonAllocated(this TimeSpan time, char[] buffer, Span<char> sBuffer) {
        var buf = buffer.Length is 0
            ? StringBuffer.Create(sBuffer)
            : StringBuffer.Create(buffer!);

        if (time.TotalSeconds <= 1) {
            buf.Append("0s");
            return buf;
        }

        if (time.Days > 0) {
            buf.Append(time.Days);
            buf.Append("d ");
        }
        if (time.Hours > 0) {
            buf.Append(time.Hours);
            buf.Append("h ");
        }
        if (time.Minutes > 0) {
            buf.Append(time.Minutes);
            buf.Append("m ");
        }
        if (time.Seconds > 0) {
            buf.Append(time.Seconds);
            buf.Append("s ");
        }
        return buf;
    }

    /// <summary>
    /// Returns a Time Stamp -> HHMM-dd-mmm-yy
    /// </summary>
    public static string ToTimeStamp(this DateTime time)
        => ToTimeStampNonAllocated(time, Array.Empty<char>(), stackalloc char[30]).Allocate(true, true);

    /// <summary>
    /// Returns a Time Stamp -> HHMM-dd-mmm-yy
    /// </summary>
    /// <remarks>
    /// <para>
    /// This version does not allocate the string, and reuses an existing buffer
    /// </para>
    /// <para>
    /// Make sure the buffer is at least 30 characters long to be safe
    /// </para>
    /// </remarks>
    public static ReadOnlySpan<char> ToTimeStampNonAllocated(this DateTime time, Span<char> buffer)
        => ToTimeStampNonAllocated(time, Array.Empty<char>(), buffer);

    /// <summary>
    /// Returns a Time Stamp -> HHMM-dd-mmm-yy
    /// </summary>
    /// <remarks>
    /// <para>
    /// This version does not allocate the string, and reuses an existing buffer
    /// </para>
    /// <para>
    /// Make sure the buffer is at least 30 characters long to be safe
    /// </para>
    /// </remarks>
    public static ReadOnlyMemory<char> ToTimeStampNonAllocated(this DateTime time, char[] buffer)
        => ToTimeStampNonAllocated(time, buffer, Span<char>.Empty);

    /// <summary>
    /// Returns a Time Stamp -> HHMM-dd-mmm-yy
    /// </summary>
    /// <remarks>
    /// <para>
    /// This version does not allocate the string, and reuses an existing buffer
    /// </para>
    /// If the buffer is empty, the span is used, and the other way around.
    /// </remarks>
    private static AllocatedStringBuffer ToTimeStampNonAllocated(this DateTime time, char[] buffer, Span<char> sBuffer) {
        var buf = buffer.Length is 0
            ? StringBuffer.Create(sBuffer)
            : StringBuffer.Create(buffer!);
        buf.Append(time.Hour);
        buf.Append(time.Minute);
        buf.Append('-');
        buf.Append(time.Day);
        buf.Append('-');
        buf.Append(CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(time.Month));
        buf.Append('-');
        buf.Append(time.Year % 100);
        return buf;
    }
}