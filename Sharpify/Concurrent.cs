namespace Sharpify;

/// <summary>
/// A wrapper struct over the collection to be processed in parallel.
/// </summary>
/// <typeparam name="T"></typeparam>
public readonly ref struct Concurrent<T> {
    internal readonly ICollection<T> Source;

    internal Concurrent(ICollection<T> source) {
        Source = source;
    }
}