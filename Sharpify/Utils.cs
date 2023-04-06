namespace Sharpify;

/// <summary>
/// Provides utility methods that are not extensions
/// </summary>
public static class Utils {
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

    /// <summary>
    /// Returns a rolling average
    /// </summary>
    /// <param name="oldVal"></param>
    /// <param name="newVal"></param>
    /// <param name="count"></param>
    public static double RollingAverage(
        double oldVal,
        double newVal,
        int count) =>
                  count is 0
                  ? newVal
                  : (oldVal * (count - 1) / count) + (newVal / count);
}