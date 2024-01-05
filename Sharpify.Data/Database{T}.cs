using System.Buffers;
using System.Collections.Concurrent;

namespace Sharpify.Data;

/// <summary>
/// A high performance database that stores string-T pairs. O(1) CRUD but O(N) Serialization.
/// </summary>
public sealed class Database<T> {
    private record KVP(string Key, T Value);

    private readonly Dictionary<string, T> _data;

    private readonly ConcurrentQueue<KVP> _queue = new();

    /// <summary>
    /// Holds the configuration for this database.
    /// </summary>
    public readonly DatabaseConfiguration<T> Config;

    /// <summary>
    /// Triggered when there is a change in the database.
    /// </summary>
    public event DataChangedEventHandler? DataChanged;

    /// <summary>
    /// Represents the method that will handle the data changed event.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">An instance of the DataChangedEventArgs class that contains the event data.</param>
    public delegate void DataChangedEventHandler(object sender, DataChangedEventArgs e);

    private void InvokeDataEvent(DataChangedEventArgs e) {
        DataChanged?.Invoke(this, e);
    }

    /// <summary>
    /// Creates a high performance database that stores string-T pairs.
    /// </summary>
    public static Database<T> Create(DatabaseConfiguration<T> config) {
        if (!File.Exists(config.Path)) {
            return new Database<T>(new(config.Options.GetComparer()), config);
        }

        Dictionary<string, T> dict = config.Path.Deserialize<T>(
                config.EncryptionKey,
                config.Options);

        return new Database<T>(dict, config);
    }

    /// <summary>
    /// Creates asynchronously a high performance database that stores string-T pairs.
    /// </summary>
    public static async ValueTask<Database<T>> CreateAsync(DatabaseConfiguration<T> config, CancellationToken token = default) {
        if (!File.Exists(config.Path)) {
            return new Database<T>(new(config.Options.GetComparer()), config);
        }

        Dictionary<string, T> dict = await config.Path.DeserializeAsync<T>(
                config.EncryptionKey,
                config.Options,
                token);

        return new Database<T>(dict, config);
    }

    private Database(Dictionary<string, T> data, DatabaseConfiguration<T> config) {
        _data = data;
        Config = config;
    }

    /// <summary>
    /// Returns the amount of entries in the database.
    /// </summary>
    public int Count => _data.Count;

    /// <summary>
    /// Checked whether the inner dictionary contains the <paramref name="key"/>.
    /// </summary>
    /// <param name="key"></param>
    public bool ContainsKey(string key) => _data.ContainsKey(key);

    /// <summary>
    /// Indexer option for the getter and setter.
    /// </summary>
    /// <param name="index"></param>
    /// <remarks>
    /// Unlike the <see cref="Dictionary{TKey, TValue}"/> this indexer will return default value if the key doesn't exist instead of throwing an exception.
    /// </remarks>
    public T? this[string index] {
        get => Get(index);
        set => Upsert(index, value!);
    }

    /// <summary>
    /// Returns the value for the <paramref name="key"/> as a byte[].
    /// </summary>
    /// <param name="key"></param>
    /// <remarks>
    /// <para>This pure method which returns the value as byte[] allows you to use more complex but also more efficient serializers
    /// </para>
    /// <para>If the value doesn't exist null is returned. You can use this to check if a value exists.</para>
    /// </remarks>
    public T? Get(string key) => _data.TryGetValue(key, out var val) ? val : default;

    /// <summary>
    /// Removes the <paramref name="key"/> and its value from the inner dictionary.
    /// </summary>
    /// <param name="key"></param>
    /// <returns>True if the key was removed, false if it didn't exist or couldn't be removed.</returns>
    public bool Remove(string key) {
        if (_data.Count is 0) {
            return false;
        }

        lock (_data) {
            if (!_data.Remove(key, out var val)) {
                return false;
            }
            if (Config.Options.HasFlag(DatabaseOptions.TriggerUpdateEvents)) {
                InvokeDataEvent(new DataChangedEventArgs {
                    Key = key,
                    Value = val,
                    ChangeType = DataChangeType.Remove
                });
            }
            if (Config.Options.HasFlag(DatabaseOptions.SerializeOnUpdate)) {
                Serialize();
            }
            return true;
        }
    }

    /// <summary>
    /// Removes all the keys and values in the dictionary for which the value passes the <paramref name="selector"/>.
    /// </summary>
    /// <param name="selector"></param>
    public void RemoveAny(Func<T, bool> selector) {
        if (_data.Count is 0) {
            return;
        }

        lock (_data) {
            int count = 0;
            var length = _data.Count;
            var array = ArrayPool<string>.Shared.Rent(length);
            _data.Keys.CopyTo(array, 0);
            ReadOnlySpan<string> keys = array.AsSpan(0, length);
            foreach (var key in keys) {
                var value = _data[key];
                if (!selector.Invoke(value)) {
                    continue;
                }
                if (!_data.Remove(key, out _)) {
                    continue;
                }
                count++;
                if (Config.Options.HasFlag(DatabaseOptions.TriggerUpdateEvents)) {
                    InvokeDataEvent(new DataChangedEventArgs() {
                        Key = key,
                        Value = value,
                        ChangeType = DataChangeType.Remove
                    });
                }
            }
            ArrayPool<string>.Shared.Return(array);
            if (count is 0 || !Config.Options.HasFlag(DatabaseOptions.SerializeOnUpdate)) {
                return;
            }
            Serialize();
        }
    }

    /// <summary>
    /// Clears all keys and values from the database.
    /// </summary>
    public void Clear() {
        _data.Clear();
        if (Config.Options.HasFlag(DatabaseOptions.SerializeOnUpdate)) {
            Serialize();
        }
        if (Config.Options.HasFlag(DatabaseOptions.TriggerUpdateEvents)) {
            InvokeDataEvent(new DataChangedEventArgs {
                Key = "ALL",
                Value = null,
                ChangeType = DataChangeType.Remove
            });
        }
    }

    /// <summary>
    /// Updates or inserts a new <paramref name="value"/> @ <paramref name="key"/>.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public void Upsert(string key, T value) {
        _queue.Enqueue(new KVP(key, value));

        if (Config.Options.HasFlag(DatabaseOptions.TriggerUpdateEvents)) {
            InvokeDataEvent(new DataChangedEventArgs {
                Key = key,
                Value = value,
                ChangeType = DataChangeType.Upsert
            });
        }

        EmptyQueue();
    }

    // Adds items to the dictionary and serializes if needed at the end.
    // This enables us to add multiple items at once without serializing multiple times.
    // Essentially synchronizing concurrent writes.
    // While the inner sequential addition to the dictionary makes it thread safe.
    private void EmptyQueue() {
        lock (_data) {
            bool itemsWereAdded = false;
            while (_queue.TryDequeue(out var kvp)) {
                _data[kvp.Key] = kvp.Value;
                itemsWereAdded = true;
            }
            if (itemsWereAdded && Config.Options.HasFlag(DatabaseOptions.SerializeOnUpdate)) {
                Serialize();
            }
        }
    }

    /// <summary>
    /// Returns an immutable copy of the keys in the inner dictionary
    /// </summary>
    public IReadOnlyCollection<string> GetKeys() => _data.Keys;

    /// <summary>
    /// Saves the database to the hard disk.
    /// </summary>
    public void Serialize() {
        lock (_data) {
            while (_queue.TryDequeue(out var kvp)) {
                _data[kvp.Key] = kvp.Value;
            }
            _data.Serialize(Config.Path, Config.EncryptionKey);
        }
    }


    /// <summary>
    /// Saves the database to the hard disk asynchronously.
    /// </summary>
    public Task SerializeAsync() {
        lock (_data) {
            while (_queue.TryDequeue(out var kvp)) {
                _data[kvp.Key] = kvp.Value;
            }
        }
        return _data.SerializeAsync(Config.Path, Config.EncryptionKey);
    }

}