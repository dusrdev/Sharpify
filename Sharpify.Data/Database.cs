using System.Buffers;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;

using MemoryPack;

namespace Sharpify.Data;

/// <summary>
/// A high performance database that stores String:byte[] pairs.
/// </summary>
/// <remarks>
/// Do not create this class directly or by using an activator, the factory methods are required for proper initializations using different abstractions.
/// </remarks>
public sealed class Database : IDisposable {
    private readonly Dictionary<string, byte[]> _data;
    private readonly ConcurrentQueue<KeyValuePair<string, byte[]>> _queue = new();

    private bool _disposed;

    private readonly ReaderWriterLockSlim _lock = new();
    private readonly DatabaseSerializer _serializer;
    private volatile int _estimatedSize;

    private const int BufferMultiple = 4096;
    private const int ReservedBufferSize = 256;

    /// <summary>
    /// Overestimated size of the database.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private int GetOverestimatedSize() {
        return (int)Math.Ceiling((_estimatedSize + ReservedBufferSize) / (double)BufferMultiple) * BufferMultiple;
    }


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
        DatabaseSerializer serializer = DatabaseSerializer.Create(config);

        if (!File.Exists(config.Path)) {
            return config.IgnoreCase
                ? new Database(new(StringComparer.OrdinalIgnoreCase), config, serializer, 0)
                : new Database(new Dictionary<string, byte[]>(), config, serializer, 0);
        }

        var estimatedSize = Extensions.GetFileSize(config.Path);

        Dictionary<string, byte[]> dict = serializer.Deserialize(estimatedSize);

        return new Database(dict, config, serializer, estimatedSize);
    }

    /// <summary>
    /// Creates asynchronously a high performance database that stores string-byte[] pairs.
    /// </summary>
    public static async ValueTask<Database> CreateAsync(DatabaseConfiguration config, CancellationToken token = default) {
        DatabaseSerializer serializer = DatabaseSerializer.Create(config);

        if (!File.Exists(config.Path)) {
            return config.IgnoreCase
                ? new Database(new(StringComparer.OrdinalIgnoreCase), config, serializer, 0)
                : new Database(new Dictionary<string, byte[]>(), config, serializer, 0);
        }

        var estimatedSize = Extensions.GetFileSize(config.Path);

        Dictionary<string, byte[]> dict = await serializer.DeserializeAsync(estimatedSize, token);

        return new Database(dict, config, serializer, estimatedSize);
    }

    private Database(Dictionary<string, byte[]> data, DatabaseConfiguration config, DatabaseSerializer serializer, int estimatedSize) {
        _data = data;
        Config = config;
        _serializer = serializer;
        Interlocked.Exchange(ref _estimatedSize, estimatedSize);
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
    public IDatabaseFilter<T> FilterByType<T>() where T : IMemoryPackable<T> => new DatabaseFilter<T>(this);


    /// <summary>
    /// Tries to get the value for the <paramref name="key"/>.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns>True if the value was found, false if not.</returns>
    public bool TryGetValue(string key, out byte[] value) => TryGetValue(key, "", out value);

    /// <summary>
    /// Tries to get the value for the <paramref name="key"/>.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="encryptionKey">individual encryption key for this specific value</param>
    /// <param name="value"></param>
    /// <returns>True if the value was found, false if not.</returns>
    public bool TryGetValue(string key, string encryptionKey, out byte[] value) {
        try {
            _lock.EnterReadLock();
            ref var val = ref _data.GetValueRefOrNullRef(key);
            if (Unsafe.IsNullRef(ref val)) {
                value = Array.Empty<byte>();
                return false;
            }
            if (encryptionKey.Length is 0) {
                value = val.FastCopy();
                return true;
            }
            value = Helper.Instance.Decrypt(val.AsSpan(), encryptionKey);
            return true;
        } finally {
            _lock.ExitReadLock();
        }
    }

    /// <summary>
    /// Tries to get the value for the <paramref name="key"/>.
    /// </summary>
    /// <typeparam name="T">The type of object to retrieve.</typeparam>
    /// <param name="key">The key used to identify the object in the database.</param>
    /// <param name="value">The retrieved object of type T, or default if the object does not exist.</param>
    /// <returns>True if the value was found, otherwise false.</returns>
    public bool TryGetValue<T>(string key, out T value) where T : IMemoryPackable<T> => TryGetValue(key, "", out value);

    /// <summary>
    /// Tries to get the value for the <paramref name="key"/>.
    /// </summary>
    /// <typeparam name="T">The type of object to retrieve.</typeparam>
    /// <param name="key">The key used to identify the object in the database.</param>
    /// <param name="encryptionKey">The encryption key used to decrypt the object if it is encrypted.</param>
    /// <param name="value">The retrieved object of type T, or default if the object does not exist.</param>
    /// <returns>True if the value was found, otherwise false.</returns>
    public bool TryGetValue<T>(string key, string encryptionKey, out T value) where T : IMemoryPackable<T> {
        try {
            _lock.EnterReadLock();
            ref var val = ref _data.GetValueRefOrNullRef(key);
            if (Unsafe.IsNullRef(ref val)) {
                value = default!;
                return false;
            }
            if (encryptionKey.Length is 0) {
                value = MemoryPackSerializer.Deserialize<T>(val.AsSpan())!;
                return true;
            }
            var buffer = ArrayPool<byte>.Shared.Rent(val.Length + AesProvider.ReservedBufferSize);
            int length = Helper.Instance.Decrypt(val.AsSpan(), buffer, encryptionKey);
            var bytes = new ReadOnlySpan<byte>(buffer, 0, length);
            value = bytes.Length is 0 ? default! : MemoryPackSerializer.Deserialize<T>(bytes)!;
            buffer.ReturnBufferToSharedArrayPool();
            return true;
        } finally {
            _lock.ExitReadLock();
        }
    }

    /// <summary>
    /// Tries to get the value array stored in <paramref name="key"/>.
    /// </summary>
    /// <typeparam name="T">The type of object to retrieve.</typeparam>
    /// <param name="key">The key used to identify the object in the database.</param>
    /// <param name="value">The retrieved object of type T, or default if the object does not exist.</param>
    /// <returns>True if the value was found, otherwise false.</returns>
    public bool TryGetValues<T>(string key, out T[] value) where T : IMemoryPackable<T> => TryGetValues(key, "", out value);

    /// <summary>
    /// Tries to get the value array stored in <paramref name="key"/>.
    /// </summary>
    /// <typeparam name="T">The type of object to retrieve.</typeparam>
    /// <param name="key">The key used to identify the object in the database.</param>
    /// <param name="encryptionKey">The encryption key used to decrypt the object if it is encrypted.</param>
    /// <param name="values">The retrieved object of type T, or default if the object does not exist.</param>
    /// <returns>True if the value was found, otherwise false.</returns>
    public bool TryGetValues<T>(string key, string encryptionKey, out T[] values) where T : IMemoryPackable<T> {
        try {
            _lock.EnterReadLock();
            ref var val = ref _data.GetValueRefOrNullRef(key);
            if (Unsafe.IsNullRef(ref val)) {
                values = default!;
                return false;
            }
            if (encryptionKey.Length is 0) {
                values = MemoryPackSerializer.Deserialize<T[]>(val.AsSpan())!;
                return true;
            }
            var buffer = ArrayPool<byte>.Shared.Rent(val.Length + AesProvider.ReservedBufferSize);
            int length = Helper.Instance.Decrypt(val.AsSpan(), buffer, encryptionKey);
            var bytes = new ReadOnlySpan<byte>(buffer, 0, length);
            values = bytes.Length is 0 ? default! : MemoryPackSerializer.Deserialize<T[]>(bytes)!;
            buffer.ReturnBufferToSharedArrayPool();
            return true;
        } finally {
            _lock.ExitReadLock();
        }
    }

    /// <summary>
    /// Tries to get the value for the <paramref name="key"/>.
    /// </summary>
    /// <param name="key">The key used to identify the object in the database.</param>
    /// <param name="value">The retrieved object of type T, or default if the object does not exist.</param>
    /// <returns>True if the value was found, otherwise false.</returns>
    public bool TryGetString(string key, out string value) => TryGetString(key, "", out value);

    /// <summary>
    /// Tries to get the value for the <paramref name="key"/>.
    /// </summary>
    /// <param name="key">The key used to identify the object in the database.</param>
    /// <param name="encryptionKey">The encryption key used to decrypt the object if it is encrypted.</param>
    /// <param name="value">The retrieved object of type T, or default if the object does not exist.</param>
    /// <returns>True if the value was found, otherwise false.</returns>
    public bool TryGetString(string key, string encryptionKey, out string value) {
        try {
            _lock.EnterReadLock();
            ref var val = ref _data.GetValueRefOrNullRef(key);
            if (Unsafe.IsNullRef(ref val)) {
                value = "";
                return false;
            }
            if (encryptionKey.Length is 0) {
                value = MemoryPackSerializer.Deserialize<string>(val.AsSpan())!;
                return true;
            }
            var buffer = ArrayPool<byte>.Shared.Rent(val.Length + AesProvider.ReservedBufferSize);
            int length = Helper.Instance.Decrypt(val.AsSpan(), buffer, encryptionKey);
            var bytes = new ReadOnlySpan<byte>(buffer, 0, length);
            value = bytes.Length is 0 ? "" : MemoryPackSerializer.Deserialize<string>(bytes)!;
            buffer.ReturnBufferToSharedArrayPool();
            return true;
        } finally {
            _lock.ExitReadLock();
        }
    }

    /// <summary>
    /// Tries to get the value for the <paramref name="key"/>.
    /// </summary>
    /// <param name="key">The key used to identify the object in the database.</param>
    /// <param name="jsonSerializerContext"></param>
    /// <param name="value">The retrieved object of type T, or default if the object does not exist.</param>
    /// <returns>True if the value was found, otherwise false.</returns>
    public bool TryGetValue<T>(string key, JsonSerializerContext jsonSerializerContext, out T value) => TryGetValue(key, "", jsonSerializerContext, out value);

    /// <summary>
    /// Tries to get the value for the <paramref name="key"/>.
    /// </summary>
    /// <param name="key">The key used to identify the object in the database.</param>
    /// <param name="encryptionKey">The encryption key used to decrypt the object if it is encrypted.</param>
    /// <param name="jsonSerializerContext"></param>
    /// <param name="value">The retrieved object of type T, or default if the object does not exist.</param>
    /// <returns>True if the value was found, otherwise false.</returns>
    public bool TryGetValue<T>(string key, string encryptionKey, JsonSerializerContext jsonSerializerContext, out T value) {
        if (!TryGetString(key, encryptionKey, out string asString)) {
            value = default!;
            return false;
        }
        value = (T)JsonSerializer.Deserialize(asString, typeof(T), jsonSerializerContext)!;
        return true;
    }

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
    [Obsolete("Use TryGetValue instead.")]
    public byte[] Get(string key, string encryptionKey = "") {
        try {
            _lock.EnterReadLock();
            ref var val = ref _data.GetValueRefOrNullRef(key);
            if (Unsafe.IsNullRef(ref val)) {
                return Array.Empty<byte>();
            }
            if (encryptionKey.Length is 0) {
                return val.FastCopy();
            }
            return Helper.Instance.Decrypt(val.AsSpan(), encryptionKey);
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
    [Obsolete("Use TryGetValue instead.")]
    public T? Get<T>(string key, string encryptionKey = "") where T : IMemoryPackable<T> {
        try {
            _lock.EnterReadLock();
            ref var val = ref _data.GetValueRefOrNullRef(key);
            if (Unsafe.IsNullRef(ref val)) {
                return default;
            }
            if (encryptionKey.Length is 0) {
                return MemoryPackSerializer.Deserialize<T>(val.AsSpan());
            }
            var buffer = ArrayPool<byte>.Shared.Rent(val.Length + AesProvider.ReservedBufferSize);
            int length = Helper.Instance.Decrypt(val.AsSpan(), buffer, encryptionKey);
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
    [Obsolete("Use TryGetValue instead.")]
    public string GetAsString(string key, string encryptionKey = "") {
        try {
            _lock.EnterReadLock();
            ref var val = ref _data.GetValueRefOrNullRef(key);
            if (Unsafe.IsNullRef(ref val)) {
                return "";
            }
            if (encryptionKey.Length is 0) {
                return MemoryPackSerializer.Deserialize<string>(val.AsSpan())!;
            }
            var buffer = ArrayPool<byte>.Shared.Rent(val.Length + AesProvider.ReservedBufferSize);
            int length = Helper.Instance.Decrypt(val.AsSpan(), buffer, encryptionKey);
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
        if (encryptionKey.Length is 0) {
            _queue.Enqueue(new(key, value.FastCopy()));
        } else {
            var encrypted = Helper.Instance.Encrypt(value.AsSpan(), encryptionKey);
            _queue.Enqueue(new(key, encrypted));
        }

        if (Config.TriggerUpdateEvents) {
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

    /// <summary>
    /// Upserts a value into the database using the specified key.
    /// </summary>
    /// <typeparam name="T">The type of the value being upserted.</typeparam>
    /// <param name="key">The key used to identify the value.</param>
    /// <param name="values">The value to be upserted.</param>
    /// <param name="encryptionKey">The encryption key used to encrypt the value.</param>
    /// <remarks>
    /// The upsert operation will either insert a new value if the key does not exist,
    /// or update the existing value if the key already exists.
    /// </remarks>
    public void UpsertMany<T>(string key, T[] values, string encryptionKey = "") where T : IMemoryPackable<T> {
        Upsert(key, MemoryPackSerializer.Serialize(values), encryptionKey);
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
    public void Upsert(string key, string value, string encryptionKey = "") {
        byte[] bytes = value.Length is 0 ?
                    Array.Empty<byte>()
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
    public void Upsert<T>(string key, T value, JsonSerializerContext jsonSerializerContext, string encryptionKey = "") where T : notnull {
        var asString = JsonSerializer.Serialize(value, typeof(T), jsonSerializerContext);
        Upsert(key, asString, encryptionKey);
    }

    // Adds items to the dictionary and serializes if needed at the end.
    // This enables us to add multiple items at once without serializing multiple times.
    // Essentially synchronizing concurrent writes.
    // While the inner sequential addition to the dictionary makes it thread safe.
    private void EmptyQueue() {
        try {
            _lock.EnterWriteLock();
            nint itemsAdded = 0;
            while (_queue.TryDequeue(out var kvp)) {
                _data[kvp.Key] = kvp.Value;
                itemsAdded++;
                var estimatedSize = kvp.GetEstimatedSize();
                Interlocked.Add(ref _estimatedSize, estimatedSize);
            }
            if (itemsAdded is not 0 && Config.SerializeOnUpdate) {
                Serialize();
            }
        } finally {
            _lock.ExitWriteLock();
        }
    }

    /// <summary>
    /// Returns an immutable copy of the keys in the inner dictionary
    /// </summary>
    public IReadOnlyCollection<string> GetKeys() {
        try {
            _lock.EnterReadLock();
            return _data.Keys;
        } finally {
            _lock.ExitReadLock();
        }
    }

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

    /// <summary>
    /// Frees the resources used by the database.
    /// </summary>
    public void Dispose() {
        if (_disposed) {
            return;
        }
        _lock.Dispose();
        _disposed = true;
    }
}