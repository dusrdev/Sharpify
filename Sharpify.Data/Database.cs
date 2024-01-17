using System.Buffers;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

using MemoryPack;

namespace Sharpify.Data;

/// <summary>
/// A high performance database that stores string-byte[] pairs.
/// </summary>
public sealed class Database : IDisposable {
    private readonly Dictionary<string, ReadOnlyMemory<byte>> _data;

    private readonly ConcurrentQueue<KeyValuePair<string, ReadOnlyMemory<byte>>> _queue = new();

    private readonly ReaderWriterLockSlim _lock = new();

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

        Dictionary<string, ReadOnlyMemory<byte>> dict = config.Path.Deserialize<ReadOnlyMemory<byte>>(
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

        Dictionary<string, ReadOnlyMemory<byte>> dict = await config.Path.DeserializeAsync<ReadOnlyMemory<byte>>(
                config.EncryptionKey,
                config.Options,
                token);

        return new Database(dict, config);
    }

    private Database(Dictionary<string, ReadOnlyMemory<byte>> data, DatabaseConfiguration config) {
        _data = data;
        Config = config;
    }

    /// <summary>
    /// Finalizes the database (releases all resources)
    /// </summary>
    ~Database() {
        Dispose();
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
    /// Returns a <see cref="DatabaseFilter{T}"/> that can be used to filter the database by type.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public DatabaseFilter<T> FilterByType<T>() where T : IMemoryPackable<T> => new(this);

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
    public ReadOnlyMemory<byte> Get(string key, string encryptionKey = "") {
        _lock.EnterReadLock();
        try {
            ref var val = ref _data.GetValueRefOrNullRef(key);
            if (Unsafe.IsNullRef(ref val)) {
                return ReadOnlyMemory<byte>.Empty;
            }
            if (encryptionKey.Length is 0) {
                return val;
            }
            return Helper.Instance.Decrypt(val.Span, encryptionKey);
        } finally {
            _lock.ExitReadLock();
        }
    }

    /// <summary>
    /// Retrieves an object of type T from the database using the specified key.
    /// </summary>
    /// <typeparam name="T">The type of object to retrieve.</typeparam>
    /// <param name="key">The key used to identify the object in the database.</param>
    /// <param name="encryptionKey">The encryption key used to decrypt the object if it is encrypted.</param>
    /// <returns>The retrieved object of type T, or null if the object does not exist.</returns>
    public T? Get<T>(string key, string encryptionKey = "") where T : IMemoryPackable<T> {
        _lock.EnterReadLock();
        try {
            ref var val = ref _data.GetValueRefOrNullRef(key);
            if (Unsafe.IsNullRef(ref val)) {
                return default;
            }
            if (encryptionKey.Length is 0) {
                return MemoryPackSerializer.Deserialize<T>(val.Span);
            }
            var buffer = ArrayPool<byte>.Shared.Rent(val.Length);
            int length = Helper.Instance.Decrypt(val.Span, buffer, encryptionKey);
            var bytes = new ReadOnlySpan<byte>(buffer, 0, length);
            var result = bytes.Length is 0 ? default : MemoryPackSerializer.Deserialize<T>(bytes)!;
            buffer.ReturnBufferToSharedArrayPool();
            return result;
        } finally {
            _lock.ExitReadLock();
        }
    }

    /// <summary>
    /// Returns the value for the <paramref name="key"/> as string. or empty string if the value doesn't exist.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="encryptionKey">individual encryption key for this specific value</param>
    public string GetAsString(string key, string encryptionKey = "") {
        _lock.EnterReadLock();
        try {
            ref var val = ref _data.GetValueRefOrNullRef(key);
            if (Unsafe.IsNullRef(ref val)) {
                return "";
            }
            if (encryptionKey.Length is 0) {
                return MemoryPackSerializer.Deserialize<string>(val.Span)!;
            }
            var buffer = ArrayPool<byte>.Shared.Rent(val.Length);
            int length = Helper.Instance.Decrypt(val.Span, buffer, encryptionKey);
            var bytes = new ReadOnlySpan<byte>(buffer, 0, length);
            var result = bytes.Length is 0 ? "" : MemoryPackSerializer.Deserialize<string>(bytes)!;
            buffer.ReturnBufferToSharedArrayPool();
            return result;
        } finally {
            _lock.ExitReadLock();
        }
    }

    /// <summary>
    /// Removes the <paramref name="key"/> and its value from the inner dictionary.
    /// </summary>
    /// <param name="key"></param>
    /// <returns>True if the key was removed, false if it didn't exist or couldn't be removed.</returns>
    public bool Remove(string key) {
        _lock.EnterWriteLock();
        try {
            if (_data.Count is 0) {
                return false;
            }
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
        } finally {
            _lock.ExitWriteLock();
        }
    }

    /// <summary>
    /// Clears all keys and values from the database.
    /// </summary>
    public void Clear() {
        _lock.EnterWriteLock();
        try {
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
        } finally {
            _lock.ExitWriteLock();
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
    public void Upsert(string key, ReadOnlyMemory<byte> value, string encryptionKey = "") {
        ReadOnlyMemory<byte> val;

        if (encryptionKey.Length is 0) {
            val = value;
        } else {
            val = Helper.Instance.Encrypt(value.Span, encryptionKey);
        }

        _queue.Enqueue(new(key, val));

        if (Config.Options.HasFlag(DatabaseOptions.TriggerUpdateEvents)) {
            InvokeDataEvent(new DataChangedEventArgs {
                Key = key,
                Value = value,
                ChangeType = DataChangeType.Upsert
            });
        }

        EmptyQueue();
    }

    /// <summary>
    /// Upserts a value into the database using the specified key.
    /// </summary>
    /// <typeparam name="T">The type of the value being upserted.</typeparam>
    /// <param name="key">The key used to identify the value.</param>
    /// <param name="value">The value to be upserted.</param>
    /// <param name="encryptionKey">The encryption key used to encrypt the value.</param>
    /// <remarks>
    /// The upsert operation will either insert a new value if the key does not exist,
    /// or update the existing value if the key already exists.
    /// </remarks>
    public void Upsert<T>(string key, T value, string encryptionKey = "") where T : IMemoryPackable<T> {
        Upsert(key, MemoryPackSerializer.Serialize(value), encryptionKey);
    }

    // Adds items to the dictionary and serializes if needed at the end.
    // This enables us to add multiple items at once without serializing multiple times.
    // Essentially synchronizing concurrent writes.
    // While the inner sequential addition to the dictionary makes it thread safe.
    private void EmptyQueue() {
        _lock.EnterWriteLock();
        try {
            nint itemsAdded = 0;
            while (_queue.TryDequeue(out var kvp)) {
                _data[kvp.Key] = kvp.Value;
                itemsAdded++;
            }
            if (itemsAdded is not 0 && Config.Options.HasFlag(DatabaseOptions.SerializeOnUpdate)) {
                Serialize();
            }
        } finally {
            _lock.ExitWriteLock();
        }
    }

    /// <summary>
    /// Updates or inserts a new <paramref name="value"/> @ <paramref name="key"/>.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="encryptionKey">individual encryption key for this specific value</param>
    /// <remarks>
    /// This is much less efficient time and memory wise than <see cref="Upsert(string, ReadOnlyMemory{byte}, string?)"/>.
    /// </remarks>
    public void UpsertAsString(string key, string value, string encryptionKey = "") {
        ReadOnlyMemory<byte> bytes = value.Length is 0 ?
                    ReadOnlyMemory<byte>.Empty
                    : MemoryPackSerializer.Serialize(value);

        Upsert(key, bytes, encryptionKey);
    }

    /// <summary>
    /// Updates or inserts a new <paramref name="value"/> @ <paramref name="key"/>.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="jsonSerializerContext">That can be used to serialize T</param>
    /// <param name="encryptionKey">individual encryption key for this specific value</param>
    /// <remarks>
    /// This is the least efficient option as it uses a reflection JSON serializer and byte conversion.
    /// </remarks>
    public void UpsertAsT<T>(string key, T value, JsonSerializerContext jsonSerializerContext, string encryptionKey = "") {
        ReadOnlyMemory<byte> bytes = value is null ?
                    ReadOnlyMemory<byte>.Empty
                    : MemoryPackSerializer.Serialize(value.Serialize(jsonSerializerContext));

        Upsert(key, bytes, encryptionKey);
    }

    /// <summary>
    /// Returns an immutable copy of the keys in the inner dictionary
    /// </summary>
    public IReadOnlyCollection<string> GetKeys() {
        _lock.EnterReadLock();
        try {
            return _data.Keys;
        } finally {
            _lock.ExitReadLock();
        }
    }

    /// <summary>
    /// Saves the database to the hard disk.
    /// </summary>
    public void Serialize() {
        if (!Config.Options.HasFlag(DatabaseOptions.SerializeOnUpdate)) {
            while (_queue.TryDequeue(out var kvp)) {
                _data[kvp.Key] = kvp.Value;
            }
        }
        _data.Serialize(Config.Path, Config.EncryptionKey);
    }

    /// <summary>
    /// Saves the database to the hard disk asynchronously.
    /// </summary>
    public Task SerializeAsync() {
        if (!Config.Options.HasFlag(DatabaseOptions.SerializeOnUpdate)) {
            while (_queue.TryDequeue(out var kvp)) {
                _data[kvp.Key] = kvp.Value;
            }
        }
        return _data.SerializeAsync(Config.Path, Config.EncryptionKey);
    }

    /// <summary>
    /// Frees the resources used by the database.
    /// </summary>
    public void Dispose() {
        _lock.Dispose();
        GC.SuppressFinalize(this);
    }
}