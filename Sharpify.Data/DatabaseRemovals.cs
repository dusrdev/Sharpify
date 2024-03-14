namespace Sharpify.Data;

public sealed partial class Database : IDisposable {
    /// <summary>
    /// Removes the <paramref name="key"/> and its value from the inner dictionary.
    /// </summary>
    /// <param name="key"></param>
    /// <returns>True if the key was removed, false if it didn't exist or couldn't be removed.</returns>
    public bool Remove(string key) {
        try {
            _lock.EnterWriteLock();
            if (!_data.Remove(key, out var val)) {
                return false;
            }
            var estimatedSize = new KeyValuePair<string, byte[]>(key, val).GetEstimatedSize();
            Interlocked.Add(ref _estimatedSize, -estimatedSize);
            if (Config.SerializeOnUpdate) {
                Serialize();
            }
            if (Config.TriggerUpdateEvents) {
                InvokeDataEvent(new DataChangedEventArgs {
                    Key = key,
                    Value = val,
                    ChangeType = DataChangeType.Remove
                });
            }
            return true;
        } finally {
            _lock.ExitWriteLock();
        }
    }

    /// <summary>
    /// Clears all keys and values from the database.
    /// </summary>
    public void Clear() {
        try {
            _lock.EnterWriteLock();
            _data.Clear();
            Interlocked.Exchange(ref _estimatedSize, 0);
            if (Config.SerializeOnUpdate) {
                Serialize();
            }
            if (Config.TriggerUpdateEvents) {
                InvokeDataEvent(new DataChangedEventArgs {
                    Key = "ALL",
                    Value = null,
                    ChangeType = DataChangeType.Remove
                });
            }
        } finally {
            _lock.ExitWriteLock();
        }
    }
}