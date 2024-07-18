namespace Sharpify;

/// <summary>
/// Represents a partition of an enumerated collection that can be asynchronously processed.
/// </summary>
/// <typeparam name="T">The type of elements in the partition.</typeparam>
internal sealed class EnumeratedPartition<T> {
    private readonly IAsyncAction<T> _action;

    public EnumeratedPartition(IAsyncAction<T> action) => _action = action;

    /// <summary>
    /// Asynchronously processes the elements in the partition.
    /// </summary>
    /// <param name="partition">The enumerator for the partition.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task AwaitPartitionAsync(IEnumerator<T> partition) {
        using (partition) {
            while (partition.MoveNext()) {
                await _action.InvokeAsync(partition.Current).ConfigureAwait(false);
            }
        }
    }
}