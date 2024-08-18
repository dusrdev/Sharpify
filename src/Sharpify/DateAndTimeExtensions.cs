using System.Globalization;

using Sharpify.Collections;

namespace Sharpify;

public static partial class Extensions {
    private const int TimeSpanRequiredBufferLength = 30;

    private static (double value, string suffix) DeconstructElapsedTimeSpan(TimeSpan elapsed) => elapsed.TotalSeconds switch {
        < 1 => (elapsed.TotalMilliseconds, "ms"),
        < 60 => (elapsed.TotalSeconds, "s"),
        < 3600 => (elapsed.TotalMinutes, "m"),
        < 86400 => (elapsed.TotalHours, "hr"),
        _ => (elapsed.TotalDays, "d")
    };

    /// <summary>
    /// Formats a <see cref="TimeSpan"/> to a pretty string
    /// </summary>
    public static string Format(this TimeSpan elapsed) {
        (double value, string suffix) = DeconstructElapsedTimeSpan(elapsed);

        return StringBuffer.Create(stackalloc char[TimeSpanRequiredBufferLength])
                           .Append(Math.Round(value, 2))
                           .Append(suffix);
    }

    /// <summary>
    /// Formats a <see cref="TimeSpan"/> to a pretty string
    /// </summary>
    /// <returns><see cref="StringBuffer"/></returns>
    /// <remarks>
    /// Make sure to dispose the buffer after use (you can use it in a using statement), view the result with <see cref="StringBuffer.WrittenSpan"/>
    /// </remarks>
    private static StringBuffer FormatInRentedBuffer(this TimeSpan elapsed) {
        (double value, string suffix) = DeconstructElapsedTimeSpan(elapsed);

        return StringBuffer.Rent(TimeSpanRequiredBufferLength)
                           .Append(Math.Round(value, 2))
                           .Append(suffix);
    }

    /// <summary>
    /// Formats time span to human readable format
    /// </summary>
    public static string ToRemainingDuration(this TimeSpan time) {
        var buffer = StringBuffer.Create(stackalloc char[TimeSpanRequiredBufferLength]);

        if (time.TotalSeconds <= 1) {
            return "0s";
        }

        if (time.Days > 0) {
            buffer.Append(time.Days)
               .Append("d ");
        }
        if (time.Hours > 0) {
            buffer.Append(time.Hours)
               .Append("h ");
        }
        if (time.Minutes > 0) {
            buffer.Append(time.Minutes)
               .Append("m ");
        }
        if (time.Seconds > 0) {
            buffer.Append(time.Seconds)
               .Append("s ");
        }

        ReadOnlySpan<char> span = buffer.WrittenSpan;
        span = span.Slice(0, span.Length - 1);
        return new string(span);
    }

    /// <summary>
    /// Formats time span to human readable format into a rented buffer
    /// </summary>
    /// <returns><see cref="StringBuffer"/></returns>
    /// <remarks>
    /// Make sure to dispose the buffer after use (you can use it in a using statement), view the result with <see cref="StringBuffer.WrittenSpan"/>
    /// </remarks>
    private static StringBuffer ToRemainingDurationInRentedBuffer(this TimeSpan time) {
        var buffer = StringBuffer.Rent(TimeSpanRequiredBufferLength);

        if (time.TotalSeconds <= 1) {
            return buffer.Append("0s");
        }

        if (time.Days > 0) {
            buffer.Append(time.Days)
               .Append("d ");
        }
        if (time.Hours > 0) {
            buffer.Append(time.Hours)
               .Append("h ");
        }
        if (time.Minutes > 0) {
            buffer.Append(time.Minutes)
               .Append("m ");
        }
        if (time.Seconds > 0) {
            buffer.Append(time.Seconds)
               .Append("s ");
        }
        return buffer;
    }

    /// <summary>
    /// Returns a Time Stamp -> HHMM-dd-mmm-yy
    /// </summary>
    public static string ToTimeStamp(this DateTime time) {
        return StringBuffer.Create(stackalloc char[TimeSpanRequiredBufferLength])
                           .Append(time.Hour)
                           .Append(time.Minute)
                           .Append('-')
                           .Append(time.Day)
                           .Append('-')
                           .Append(CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(time.Month))
                           .Append('-')
                           .Append(time.Year % 100);
    }


    /// <summary>
    /// Returns a Time Stamp (HHMM-dd-mmm-yy) formatted into a rented buffer
    /// </summary>
    /// <returns><see cref="StringBuffer"/></returns>
    /// <remarks>
    /// Make sure to dispose the buffer after use (you can use it in a using statement), view the result with <see cref="StringBuffer.WrittenSpan"/>
    /// </remarks>
    private static StringBuffer ToTimeStampInRentedBuffer(this DateTime time) {
        return StringBuffer.Rent(TimeSpanRequiredBufferLength)
                           .Append(time.Hour)
                           .Append(time.Minute)
                           .Append('-')
                           .Append(time.Day)
                           .Append('-')
                           .Append(CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(time.Month))
                           .Append('-')
                           .Append(time.Year % 100);
    }
}