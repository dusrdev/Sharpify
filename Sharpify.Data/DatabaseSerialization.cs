namespace Sharpify.Data;

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