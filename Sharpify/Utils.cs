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
    /// <param name="oldAverage"></param>
    /// <param name="newNumber"></param>
    /// <param name="sampleCount"></param>
    public static double RollingAverage(double oldAverage, double newNumber, int sampleCount) {
        if (sampleCount < 0)
            throw new ArgumentException("Count must be greater than or equal to 0", nameof(sampleCount));

        if (sampleCount is 0)
            return newNumber;

        double denominator = 1 / (double)sampleCount;
        return (oldAverage * (sampleCount - 1) * denominator) + (newNumber * denominator);
    }
}