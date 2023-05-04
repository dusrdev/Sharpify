namespace Sharpify;

/// <summary>
/// A wrapper struct over the collection to be processed in parallel.
/// </summary>
/// <typeparam name="T"></typeparam>
public readonly ref struct Concurrent<T> {
    internal readonly ICollection<T> Source;

    /// <summary>
    /// Concurrent cannot be instantiated directly. Use Extension method for ICollections instead.
    /// </summary>
    public Concurrent() => throw new InvalidOperationException("Concurrent cannot be instantiated directly. Use Extension method for ICollections instead.");

    internal Concurrent(ICollection<T> source) {
        Source = source;
    }
}