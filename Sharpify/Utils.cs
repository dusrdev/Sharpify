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
}