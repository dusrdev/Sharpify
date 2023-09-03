using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace Sharpify;

public static partial class Extensions {
    /// <summary>
    /// Formats a <see cref="TimeSpan"/> to a pretty string
    /// </summary>
    /// <param name="elapsed"></param>
    public static string Format(this TimeSpan elapsed) => elapsed.TotalSeconds switch {
        < 1 => $"{Math.Round(elapsed.TotalMilliseconds, 2)}ms",
        < 60 => $"{Math.Round(elapsed.TotalSeconds, 2)}s",
        < 3600 => $"{Math.Round(elapsed.TotalMinutes, 2)}m",
        < 86400 => $"{Math.Round(elapsed.TotalHours, 2)}hr",
        _ => $"{Math.Round(elapsed.TotalDays, 2)}d"
    };

    private static readonly ThreadLocal<StringBuilder> RemainingTimeBuilder = new(static () => new StringBuilder());

    /// <summary>
    /// Formats time span to human readable format
    /// </summary>
    /// <param name="time"></param>
    public static string ToRemainingDuration(this TimeSpan time) {
        if (time.TotalSeconds <= 1) {
            return "0s";
        }

        var remainingTimeBuilder = RemainingTimeBuilder.Value;
        remainingTimeBuilder!.Clear();

        if (time.Days > 0) {
            remainingTimeBuilder.Append(time.Days).Append("d ");
        }
        if (time.Hours > 0) {
            remainingTimeBuilder.Append(time.Hours).Append("h ");
        }
        if (time.Minutes > 0) {
            remainingTimeBuilder.Append(time.Minutes).Append("m ");
        }
        if (time.Seconds > 0) {
            remainingTimeBuilder.Append(time.Seconds).Append("s ");
        }

        Debug.Assert(remainingTimeBuilder.Length > 0);

        if (remainingTimeBuilder.Length >= 0) {
            remainingTimeBuilder.Length--;
        }

        return remainingTimeBuilder.ToString();
    }

    /// <summary>
    /// Returns a Time Stamp -> HHMM-dd-mmm-yy
    /// </summary>
    /// <param name="time"></param>
    public static string ToTimeStamp(this DateTime time) {
        Span<char> buffer = stackalloc char[14];
        const char zero = '0';

        // Append the hour and minute to the buffer
        buffer[0] = (char)(zero + (time.Hour * 0.1));
        buffer[1] = (char)(zero + (time.Hour % 10));
        buffer[2] = (char)(zero + (time.Minute * 0.1));
        buffer[3] = (char)(zero + (time.Minute % 10));

        // Append the day
        var monthAbbreviation = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(time.Month);
        buffer[4] = '-';
        buffer[5] = (char)(zero + (time.Day * 0.1));
        buffer[6] = (char)(zero + (time.Day % 10));
        // Append the month abbreviation
        buffer[7] = '-';
        buffer[8] = monthAbbreviation[0];
        buffer[9] = monthAbbreviation[1];
        buffer[10] = monthAbbreviation[2];
        // Append the year
        buffer[11] = '-';
        buffer[12] = (char)(zero + (time.Year % 100 * 0.1));
        buffer[13] = (char)(zero + (time.Year % 10));

        return new string(buffer);
    }
}