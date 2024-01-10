namespace Sharpify;

/// <summary>
/// Interface to implement for a parallel action.
/// </summary>
public interface IAction<in T> {
    /// <summary>
    /// Main entry point for the action.
    /// </summary>
    void Invoke(T item);
}

/// <summary>
/// Interface to implement for a parallel async action.
/// </summary>
public interface IAsyncAction<in T> {
    /// <summary>
    /// The main action to be performed.
    /// </summary>
    Task InvokeAsync(T item);
}

/// <summary>
/// Interface to implement for a parallel async action on value task.
/// </summary>
public interface IAsyncValueAction<in T> {
    /// <summary>
    /// The main action to be performed.
    /// </summary>
    ValueTask InvokeAsync(T item);
}