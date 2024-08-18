namespace Sharpify;

/// <summary>
/// Represents a single-writer, reader bound channel for precise batched parallelism.
/// </summary>
/// <typeparam name="T"></typeparam>
/// <remarks>
/// <para>
/// This abstraction is useful to concurrently handle precise batches of tasks running on a single collection of data.
/// </para>
/// <para>
/// This channel is not an alternative to <see cref="Task.WhenAll(IEnumerable{Task})"/>, for small and quick tasks, this would likely be slower and have more overhead. But for long running complex tasks where batch size is important, this abstraction can be very powerful.
/// </para>
/// </remarks>
public class ReaderBoundChannel<T> {

}