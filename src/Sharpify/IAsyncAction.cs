namespace Sharpify;

/// <summary>
/// Interface to implement for a parallel async action.
/// </summary>
public interface IAsyncAction<in T> {
    /// <summary>
    /// The main action to be performed.
    /// </summary>
    Task InvokeAsync(T item, CancellationToken token = default);
}