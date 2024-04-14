using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

using MemoryPack;

namespace Sharpify.Data;

public sealed partial class Database : IDisposable {
    /// <summary>
    /// Updates or inserts a new <paramref name="value"/> @ <paramref name="key"/>.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="encryptionKey">individual encryption key for this specific value</param>
    /// <remarks>
    /// This pure method which accepts the value as byte[] allows you to use more complex but also more efficient serializers.
    /// </remarks>
    public void Upsert(string key, byte[]? value, string encryptionKey = "") {
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
        Upsert(key, MemoryPackSerializer.Serialize(value, _serializer.SerializerOptions), encryptionKey);
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
        Upsert(key, MemoryPackSerializer.Serialize(values, _serializer.SerializerOptions), encryptionKey);
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
                    : MemoryPackSerializer.Serialize(value, _serializer.SerializerOptions);

        Upsert(key, bytes, encryptionKey);
    }

    /// <summary>
    /// Updates or inserts a new <paramref name="value"/> @ <paramref name="key"/>.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="jsonTypeInfo">That can be used to serialize T</param>
    /// <param name="encryptionKey">individual encryption key for this specific value</param>
    /// <remarks>
    /// This is the least efficient option as it uses a reflection JSON serializer and byte conversion.
    /// </remarks>
    public void Upsert<T>(string key, T value, JsonTypeInfo<T> jsonTypeInfo, string encryptionKey = "") where T : notnull {
        var asString = JsonSerializer.Serialize(value, jsonTypeInfo);
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