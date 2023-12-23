using System.Buffers;
using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace Sharpify;

public static partial class Extensions {
    /// <summary>
    /// Formats a <see cref="TimeSpan"/> to a pretty string
    /// </summary>
    public static string Format(this TimeSpan elapsed) {
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
        var arr = ArrayPool<char>.Shared.Rent(25); // and 2 for the suffix
        Span<char> buffer = arr;
        int index = 0;
        Math.Round(value, 2).TryFormat(buffer, out var charsWritten);
        index += charsWritten;
        suffix.CopyTo(buffer[index..]);
        index += suffix.Length;
        var res = new string(buffer[0..index]);
        ArrayPool<char>.Shared.Return(arr);
        return res;
    }

    private static readonly ThreadLocal<StringBuilder> RemainingTimeBuilder = new(static () => new StringBuilder());

    /// <summary>
    /// Formats time span to human readable format
    /// </summary>
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

        if (remainingTimeBuilder.Length > 0) {
            remainingTimeBuilder.Length--;
        }

        return remainingTimeBuilder.ToString();
    }

    /// <summary>
    /// Returns a Time Stamp -> HHMM-dd-mmm-yy
    /// </summary>
    public static string ToTimeStamp(this DateTime time) {
        const int length = 14;
        var arr = ArrayPool<char>.Shared.Rent(length);
        Span<char> buffer = arr;
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

        var res = new string(buffer[..length]);
        ArrayPool<char>.Shared.Return(arr);
        return res;
    }
}