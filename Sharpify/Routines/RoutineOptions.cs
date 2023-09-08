namespace Sharpify.Routines;

/// <summary>
/// Options that can be used to configure the behavior of an async routine.
/// </summary>
public enum RoutineOptions : byte {
    /// <summary>
    /// Represents the possible states of an asynchronous routine.
    /// </summary>
    None = 0,

    /// <summary>
    /// Flag that indicates whether the async routine should execute all the actions in parallel.
    /// </summary>
    ExecuteInParallel = 1 << 0,

    /// <summary>
    /// Flag indicating whether to throw an exception when the async routine is cancelled.
    /// </summary>
    ThrowOnCancellation = 1 << 1
}