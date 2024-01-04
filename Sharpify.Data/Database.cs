using System.Collections.Concurrent;

namespace Sharpify.Data;

/// <summary>
/// A high performance database that stores string-byte[] pairs.
/// </summary>
public sealed class Database {
    private record KVP(string Key, byte[] Value);

    private readonly Dictionary<string, byte[]> _data;

    private readonly ConcurrentQueue<KVP> _queue = new();

    /// <summary>
    /// Holds the configuration for this database.
    /// </summary>
    public readonly DatabaseConfiguration Config;

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
    /// Creates a high performance database that stores string-byte[] pairs.
    /// </summary>
    public static Database Create(DatabaseConfiguration config) {
        if (!File.Exists(config.Path)) {
            return new Database(new(config.Options.GetComparer()), config);
        }

        Dictionary<string, byte[]> dict = config.Path.Deserialize<byte[]>(
                config.EncryptionKey,
                config.Options);

        return new Database(dict, config);
    }

    /// <summary>
    /// Creates asynchronously a high performance database that stores string-byte[] pairs.
    /// </summary>
    public static async ValueTask<Database> CreateAsync(DatabaseConfiguration config, CancellationToken token = default) {
        if (!File.Exists(config.Path)) {
            return new Database(new(config.Options.GetComparer()), config);
        }

        Dictionary<string, byte[]> dict = await config.Path.DeserializeAsync<byte[]>(
                config.EncryptionKey,
                config.Options,
                token);

        return new Database(dict, config);
    }

    private Database(Dictionary<string, byte[]> data, DatabaseConfiguration config) {
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
    /// Returns the value for the <paramref name="key"/> as a byte[].
    /// </summary>
    /// <param name="key"></param>
    /// <param name="encryptionKey">individual encryption key for this specific value</param>
    /// <remarks>
    /// <para>This pure method which returns the value as byte[] allows you to use more complex but also more efficient serializers
    /// </para>
    /// <para>If the value doesn't exist null is returned. You can use this to check if a value exists.</para>
    /// </remarks>
    public ReadOnlySpan<byte> Get(string key, string encryptionKey = "") {
        if (!_data.TryGetValue(key, out var val)) {
            return default;
        }
        if (encryptionKey.Length is 0) {
            return val;
        }
        return Helper.Instance.Decrypt(val, encryptionKey);
    }

    /// <summary>
    /// Returns the value for the <paramref name="key"/> as string. or empty string if the value doesn't exist.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="encryptionKey">individual encryption key for this specific value</param>
    public string GetAsString(string key, string encryptionKey = "") {
        var bytes = Get(key, encryptionKey);
        if (bytes.Length is 0) {
            return "";
        }
        return bytes.ToUtf8String();
    }

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
            if (Config.Options.HasFlag(DatabaseOptions.SerializeOnUpdate)) {
                Serialize();
            }
            if (Config.Options.HasFlag(DatabaseOptions.TriggerUpdateEvents)) {
                InvokeDataEvent(new DataChangedEventArgs {
                    Key = key,
                    Value = val,
                    ChangeType = DataChangeType.Remove
                });
            }
            return true;
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
    /// <param name="encryptionKey">individual encryption key for this specific value</param>
    /// <remarks>
    /// This pure method which accepts the value as byte[] allows you to use more complex but also more efficient serializers.
    /// </remarks>
    public void Upsert(string key, byte[] value, string encryptionKey = "") {
        var val = encryptionKey.Length is 0 ? value : Helper.Instance.Decrypt(value, encryptionKey);

        _queue.Enqueue(new KVP(key, val));

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
                _data.Add(kvp.Key, kvp.Value);
                itemsWereAdded = true;
            }
            if (itemsWereAdded && Config.Options.HasFlag(DatabaseOptions.SerializeOnUpdate)) {
                Serialize();
            }
        }
    }

    /// <summary>
    /// Updates or inserts a new <paramref name="value"/> @ <paramref name="key"/>.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="encryptionKey">individual encryption key for this specific value</param>
    /// <remarks>
    /// This is much less efficient time and memory wise than <see cref="Upsert(string, byte[], string?)"/>.
    /// </remarks>
    public void UpsertAsString(string key, string value, string encryptionKey = "") {
        var bytes = string.IsNullOrEmpty(value) ?
                    Array.Empty<byte>()
                    : value.ToByteArray();

        Upsert(key, bytes, encryptionKey);
    }

    /// <summary>
    /// Updates or inserts a new <paramref name="value"/> @ <paramref name="key"/>.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="encryptionKey">individual encryption key for this specific value</param>
    /// <remarks>
    /// This is the least efficient option as it uses a reflection JSON serializer and byte conversion.
    /// </remarks>
    public void UpsertAsT<T>(string key, T value, string encryptionKey = "") {
        var bytes = value is null ?
                    Array.Empty<byte>()
                    : value.Serialize().ToByteArray();

        Upsert(key, bytes, encryptionKey);
    }

    /// <summary>
    /// Returns an immutable copy of the keys in the inner dictionary
    /// </summary>
    public IReadOnlyCollection<string> GetKeys() => _data.Keys;

    /// <summary>
    /// Saves the database to the hard disk.
    /// </summary>
    public void Serialize() => _data.Serialize(Config.Path, Config.EncryptionKey);

    /// <summary>
    /// Saves the database to the hard disk asynchronously.
    /// </summary>
    public Task SerializeAsync() => _data.SerializeAsync(Config.Path, Config.EncryptionKey);
}