namespace Sharpify.Data;

public sealed partial class Database {
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
            var estimatedSize = Helper.GetEstimatedSize(key, val);
            Interlocked.Add(ref _estimatedSize, -estimatedSize);
            Interlocked.Increment(ref _updatesCount);
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
    /// Removes all keys that match the <paramref name="keySelector"/>.
    /// </summary>
    /// <param name="keySelector">A predicate for the key</param>
    /// <remarks>
    /// <para>
    /// This method is thread-safe and will lock the database while removing the keys.
    /// </para>
    /// <para>
    /// If TriggerUpdateEvents is enabled, this method will trigger a <see cref="DataChangedEventArgs"/> event for each key removed.
    /// </para>
    /// </remarks>
    public void Remove(Func<string, bool> keySelector) => Remove(keySelector, null);

    /// <summary>
    /// Removes all keys that match the <paramref name="keySelector"/>.
    /// </summary>
    /// <param name="keySelector">A predicate for the key</param>
    /// <param name="keyPrefix">A prefix to be removed from the keys prior to the keySelector (mainly used for IDatabaseFilter implementations), leaving it as null will skip pre-filtering</param>
    /// <remarks>
    /// <para>
    /// This method is thread-safe and will lock the database while removing the keys.
    /// </para>
    /// <para>
    /// If TriggerUpdateEvents is enabled, this method will trigger a <see cref="DataChangedEventArgs"/> event for each key removed.
    /// </para>
    /// </remarks>
    public void Remove(Func<string, bool> keySelector, string? keyPrefix) {
        try {
            _lock.EnterWriteLock();

            var predicate = keyPrefix is null
                ? keySelector
                : key => key.StartsWith(keyPrefix) && keySelector(key.Substring(keyPrefix.Length));

            var matches = _data.Keys.Where(predicate);

            foreach (var key in matches) {
                _data.Remove(key, out var val);
                var estimatedSize = Helper.GetEstimatedSize(key, val);
                Interlocked.Add(ref _estimatedSize, -estimatedSize);
                Interlocked.Increment(ref _updatesCount);

                if (Config.TriggerUpdateEvents) {
                    InvokeDataEvent(new DataChangedEventArgs {
                        Key = key,
                        Value = val,
                        ChangeType = DataChangeType.Remove
                    });
                }
            }

            if (Config.SerializeOnUpdate) {
                Serialize();
            }

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
            Interlocked.Increment(ref _updatesCount);
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