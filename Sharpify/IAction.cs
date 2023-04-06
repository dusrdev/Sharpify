namespace Sharpify;

/// <summary>
/// Interface to implement for a parallel action.
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IAction<in T> {
    /// <summary>
    /// Main entry point for the action.
    /// </summary>
    /// <param name="item"></param>
    void Invoke(T item);
}

/// <summary>
/// Interface to implement for a parallel async action.
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IAsyncAction<in T> {
    /// <summary>
    /// The main action to be performed.
    /// </summary>
    /// <param name="item"></param>
    Task InvokeAsync(T item);
}