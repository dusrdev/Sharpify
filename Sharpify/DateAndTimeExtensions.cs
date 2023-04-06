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
        _ => $"{Math.Round(elapsed.TotalMinutes, 2)}m"
    };

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

        // Append the hour and minute to the buffer
        buffer[0] = (char)('0' + (time.Hour / 10));
        buffer[1] = (char)('0' + (time.Hour % 10));
        buffer[2] = (char)('0' + (time.Minute / 10));
        buffer[3] = (char)('0' + (time.Minute % 10));

        // Append the day
        var monthAbbreviation = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(time.Month);
        buffer[4] = '-';
        buffer[5] = (char)('0' + (time.Day / 10));
        buffer[6] = (char)('0' + (time.Day % 10));
        // Append the month abbreviation
        buffer[7] = '-';
        buffer[8] = monthAbbreviation[0];
        buffer[9] = monthAbbreviation[1];
        buffer[10] = monthAbbreviation[2];
        // Append the year
        buffer[11] = '-';
        buffer[12] = (char)('0' + (time.Year % 100 / 10));
        buffer[13] = (char)('0' + (time.Year % 10));

        return new string(buffer);
    }
}