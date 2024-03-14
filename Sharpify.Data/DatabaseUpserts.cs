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
public sealed partial class Database : IDisposable {
    /// <summary>
    /// Performs an atomic upsert operation on the database. While this key is in use, other threads cannot access its value.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="transform"></param>
    /// <param name="encryptionKey"></param>
    /// <returns>The result of the processor, which if successful, contains the new value for this key</returns>
    /// <remarks>
    /// This method should only be used in specific scenarios where you need to ensure that the processing always happens on the latest value. If <see cref="Result{T}"/> is misused, such as the value is null for success, an exception will be thrown.
    /// </remarks>
    public Result<byte[]> AtomicUpsert(string key, Func<byte[], Result<byte[]>> transform, string encryptionKey = "") {
        // Create semaphore and grant access to one
        var semaphore = _semaphores.GetOrAdd(key, new SemaphoreSlim(1, 1));
        // semaphore.Wait();
        try {
            TryGetValue(key, encryptionKey, out byte[] val); // semaphore waited inside using the dictionary
            val ??= Array.Empty<byte>();
            var result = transform(val);
            if (result.IsOk) {
                ArgumentNullException.ThrowIfNull(result.Value);
                Upsert(key, result.Value, encryptionKey);
            }
            return result;
        } finally {
            semaphore.Release(); // release the semaphore after the transformation
            _semaphores.TryRemove(key, out _); // remove the semaphore from the dictionary
        }
    }

    /// <summary>
    /// Performs an atomic upsert operation on the database. While this key is in use, other threads cannot access its value.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="transform"></param>
    /// <param name="encryptionKey"></param>
    /// <returns>The result of the processor, which if successful, contains the new value for this key</returns>
    /// <remarks>
    /// This method should only be used in specific scenarios where you need to ensure that the processing always happens on the latest value. If <see cref="Result{T}"/> is misused, such as the value is null for success, an exception will be thrown.
    /// </remarks>
    public Result<T> AtomicUpsert<T>(string key, Func<T, Result<T>> transform, string encryptionKey = "") where T : IMemoryPackable<T> {
        // Create semaphore and grant access to one
        var semaphore = _semaphores.GetOrAdd(key, new SemaphoreSlim(1, 1));
        // semaphore.Wait();
        try {
            TryGetValue(key, encryptionKey, out T val); // semaphore waited inside using the dictionary
            val ??= default!;
            var result = transform(val);
            if (result.IsOk) {
                ArgumentNullException.ThrowIfNull(result.Value);
                Upsert(key, result.Value, encryptionKey);
            }
            return result;
        } finally {
            semaphore.Release(); // release the semaphore after the transformation
            _semaphores.TryRemove(key, out _); // remove the semaphore from the dictionary
        }
    }

    /// <summary>
    /// Performs an atomic upsert operation on the database. While this key is in use, other threads cannot access its value.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="transform"></param>
    /// <param name="encryptionKey"></param>
    /// <returns>The result of the processor, which if successful, contains the new value for this key</returns>
    /// <remarks>
    /// This method should only be used in specific scenarios where you need to ensure that the processing always happens on the latest value. If <see cref="Result{T}"/> is misused, such as the value is null for success, an exception will be thrown.
    /// </remarks>
    public Result<T[]> AtomicUpsertMany<T>(string key, Func<T[], Result<T[]>> transform, string encryptionKey = "") where T : IMemoryPackable<T> {
        // Create semaphore and grant access to one
        var semaphore = _semaphores.GetOrAdd(key, new SemaphoreSlim(1, 1));
        // semaphore.Wait();
        try {
            TryGetValues(key, encryptionKey, out T[] val); // semaphore waited inside using the dictionary
            val ??= default!;
            var result = transform(val);
            if (result.IsOk) {
                ArgumentNullException.ThrowIfNull(result.Value);
                UpsertMany(key, result.Value, encryptionKey);
            }
            return result;
        } finally {
            semaphore.Release(); // release the semaphore after the transformation
            _semaphores.TryRemove(key, out _); // remove the semaphore from the dictionary
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
}