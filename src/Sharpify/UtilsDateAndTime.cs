using System.Globalization;

namespace Sharpify;

public static partial class Utils {
    /// <summary>
	/// A format for timestamps
	/// </summary>
    public const string TimeStampFormat = "HHMM-dd-MMM-yy";

    /// <summary>
    /// Returns a <see cref="ReadOnlySpan{Char}"/> slice over <paramref name="time"/> formatted as <see cref="TimeStampFormat"/>
    /// </summary>
    /// <remarks>
    /// Ensure capacity >= 30
    /// </remarks>
    public static ReadOnlySpan<char> FormatTimeStamp(DateTime time, Span<char> buffer) {
        if (!time.TryFormat(buffer, out int written, TimeStampFormat, CultureInfo.CurrentCulture)) {
            return ReadOnlySpan<char>.Empty;
        }
        return buffer.Slice(0, written);
    }

    /// <summary>
    /// Returns <paramref name="time"/> formatted as <see cref="TimeStampFormat"/>
    /// </summary>
    public static string FormatTimeStamp(DateTime time) => time.ToString(TimeStampFormat);

    /// <summary>
	/// Returns a <see cref="TimeSpan"/> of the remaining time based on <paramref name="currentPercentage"/> and <paramref name="elapsed"/>.
	/// </summary>
	/// <param name="currentPercentage">Must be between 0 and 1 (inclusive)</param>
	/// <param name="elapsed">The time elapsed so far</param>
	/// <returns></returns>
    public static TimeSpan GetRemainingTime(double currentPercentage, TimeSpan elapsed) {
        if (currentPercentage >= 1) return TimeSpan.Zero;
        if (currentPercentage <= 0) return TimeSpan.MaxValue;
        var rem = (1 - currentPercentage) / currentPercentage;
        return rem * elapsed;
    }
}