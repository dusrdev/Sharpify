namespace Sharpify.Data;

public sealed partial class Database : IDisposable {
    private void EnsureUpsertsAreFinished() {
        if (!Config.SerializeOnUpdate) {
            while (_queue.TryDequeue(out var kvp)) {
                _data[kvp.Key] = kvp.Value;
                int estimatedSize = kvp.GetEstimatedSize();
                Interlocked.Add(ref _estimatedSize, estimatedSize);
                Interlocked.Increment(ref _updatesCount);
            }
        }
    }

    /// <summary>
    /// Checks if the database needs to be serialized.
    /// </summary>
    /// <returns></returns>
    private bool IsSerializationNecessary() {
        long updateCount = Interlocked.Read(ref _updatesCount);
        long prevReference = Interlocked.CompareExchange(ref _serializationReference, updateCount, _serializationReference);
        return prevReference != updateCount;
    }

    /// <summary>
    /// Saves the database to the hard disk.
    /// </summary>
    public void Serialize() {
        EnsureUpsertsAreFinished();

        if (!IsSerializationNecessary()) {
            return;
        }

        int estimatedSize = GetOverestimatedSize();
        _serializer.Serialize(_data, estimatedSize);
    }

    /// <summary>
    /// Saves the database to the hard disk asynchronously.
    /// </summary>
    public ValueTask SerializeAsync(CancellationToken cancellationToken = default) {
        EnsureUpsertsAreFinished();

        if (!IsSerializationNecessary()) {
            return ValueTask.CompletedTask;
        }

        int estimatedSize = GetOverestimatedSize();
        return _serializer.SerializeAsync(_data, estimatedSize, cancellationToken);
    }
}