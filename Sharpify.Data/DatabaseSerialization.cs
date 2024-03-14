namespace Sharpify.Data;

/// <summary>
/// A high performance database that stores String:byte[] pairs.
/// </summary>
/// <remarks>
/// Do not create this class directly or by using an activator, the factory methods are required for proper initializations using different abstractions.
/// </remarks>
public sealed partial class Database : IDisposable {
    /// <summary>
    /// Saves the database to the hard disk.
    /// </summary>
    public void Serialize() {
        if (!Config.SerializeOnUpdate) {
            while (_queue.TryDequeue(out var kvp)) {
                _data[kvp.Key] = kvp.Value;
                var estimatedSize = kvp.GetEstimatedSize();
                Interlocked.Add(ref _estimatedSize, estimatedSize);
            }
        }
        _serializer.Serialize(_data, GetOverestimatedSize());
    }

    /// <summary>
    /// Saves the database to the hard disk asynchronously.
    /// </summary>
    public ValueTask SerializeAsync(CancellationToken cancellationToken = default) {
        if (!Config.SerializeOnUpdate) {
            while (_queue.TryDequeue(out var kvp)) {
                _data[kvp.Key] = kvp.Value;
                var estimatedSize = kvp.GetEstimatedSize();
                Interlocked.Add(ref _estimatedSize, estimatedSize);
            }
        }
        return _serializer.SerializeAsync(_data, GetOverestimatedSize(), cancellationToken);
    }
}