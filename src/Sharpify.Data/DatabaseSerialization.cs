namespace Sharpify.Data;

public sealed partial class Database {
    /// <summary>
    /// Checks if the database needs to be serialized.
    /// </summary>
    /// <returns></returns>
    private bool IsSerializationNecessary() {
        if (_updatesCount == _serializationReference) {
            return false;
        }
        _serializationReference = _updatesCount;
        return true;
    }

    /// <summary>
    /// Saves the database to the hard disk.
    /// </summary>
    public void Serialize() {
        if (_isInMemory) {
            return;
        }

        lock (_lock) {
            if (!IsSerializationNecessary()) {
                return;
            }

            int estimatedSize = GetOverestimatedSize();
            _serializer.Serialize(_data, estimatedSize);
        }
    }

    /// <summary>
    /// Saves the database to the hard disk asynchronously.
    /// </summary>
    public async ValueTask SerializeAsync(CancellationToken cancellationToken = default) {
        if (_isInMemory) {
            return;
        }

        try {
            await _semaphore.WaitAsync(cancellationToken);
            if (!IsSerializationNecessary()) {
                return;
            }

            await _serializer.SerializeAsync(_data, cancellationToken);
        } finally {
            _semaphore.Release();
        }
    }
}