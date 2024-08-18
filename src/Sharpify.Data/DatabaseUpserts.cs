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
    /// <para>
    /// This pure method which accepts the value as ReadOnlySpan{byte} allows you to use more complex but also more efficient serializers.
    /// </para>
    /// </remarks>
    public void Upsert(string key, scoped ref readonly ReadOnlySpan<byte> value, string encryptionKey = "") {
        if (encryptionKey.Length is 0) {
            _queue.Enqueue(new(key, value.ToArray()));
        } else {
            byte[] encrypted = Helper.Instance.Encrypt(in value, encryptionKey);
            _queue.Enqueue(new(key, encrypted));
        }

        if (Config.TriggerUpdateEvents) {
            InvokeDataEvent(new DataChangedEventArgs {
                Key = key,
                Value = value.ToArray(),
                ChangeType = DataChangeType.Upsert
            });
        }

        EmptyQueue();
    }

    /// <summary>
    /// Updates or inserts a new <paramref name="value"/> @ <paramref name="key"/>.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="encryptionKey">individual encryption key for this specific value</param>
    /// <remarks>
    /// <para>
    /// This method directly inserts the array reference in to the database to reduce copying.
    /// </para>
    /// <para>
    /// If you cannot ensure that this reference doesn't change, for example if using a pooled array, use the <see cref="Upsert(string, ref readonly ReadOnlySpan{byte}, string)"/> method instead.
    /// </para>
    /// </remarks>
    public void Upsert(string key, byte[] value, string encryptionKey = "") {
        if (encryptionKey.Length is 0) {
            _queue.Enqueue(new(key, value));
        } else {
            scoped ReadOnlySpan<byte> valueSpan = value;
            byte[] encrypted = Helper.Instance.Encrypt(in valueSpan, encryptionKey);
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
    /// <para>
    /// Null values are disallowed and will cause an exception to be thrown.
    /// </para>
    /// </remarks>
    public void Upsert<T>(string key, T value, string encryptionKey = "") where T : IMemoryPackable<T> {
        ArgumentNullException.ThrowIfNull(value, nameof(value));
        byte[] bytes = MemoryPackSerializer.Serialize(value, _serializer.SerializerOptions);
        Upsert(key, bytes, encryptionKey);
    }

    /// <summary>
    /// Upserts values into the database using the specified key.
    /// </summary>
    /// <typeparam name="T">The type of the values being upserted.</typeparam>
    /// <param name="key">The key used to identify the values.</param>
    /// <param name="values">The values to be upserted.</param>
    /// <param name="encryptionKey">The encryption key used to encrypt the values.</param>
    /// <remarks>
    /// The upsert operation will either insert if the key does not exist,
    /// or update the existing values if the key already exists.
    /// <para>
    /// Null values are disallowed and will cause an exception to be thrown.
    /// </para>
    /// </remarks>
    public void UpsertMany<T>(string key, T[] values, string encryptionKey = "") where T : IMemoryPackable<T> {
        ArgumentNullException.ThrowIfNull(values, nameof(values));
        byte[] bytes = MemoryPackSerializer.Serialize(values, _serializer.SerializerOptions);
        Upsert(key, bytes, encryptionKey);
    }

    /// <summary>
    /// Upserts values into the database using the specified key.
    /// </summary>
    /// <typeparam name="T">The type of the values being upserted.</typeparam>
    /// <param name="key">The key used to identify the values.</param>
    /// <param name="values">The values to be upserted.</param>
    /// <param name="encryptionKey">The encryption key used to encrypt the values.</param>
    /// <remarks>
    /// The upsert operation will either insert if the key does not exist,
    /// or update the existing values if the key already exists.
    /// <para>
    /// Null values are disallowed and will cause an exception to be thrown.
    /// </para>
    /// </remarks>
    public void UpsertMany<T>(string key, ReadOnlySpan<T> values, string encryptionKey = "") where T : IMemoryPackable<T> {
        T[] array = values.ToArray();
        byte[] bytes = MemoryPackSerializer.Serialize(array, _serializer.SerializerOptions);
        Upsert(key, bytes, encryptionKey);
    }

    /// <summary>
    /// Updates or inserts a new <paramref name="value"/> @ <paramref name="key"/>.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="encryptionKey">individual encryption key for this specific value</param>
    /// <remarks>
    /// <para>
    /// Null values are disallowed and will cause an exception to be thrown.
    /// </para>
    /// </remarks>
    public void Upsert(string key, string value, string encryptionKey = "") {
        ArgumentNullException.ThrowIfNull(value, nameof(value));
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
    /// <para>
    /// Null values are disallowed and will cause an exception to be thrown.
    /// </para>
    /// </remarks>
    public void Upsert<T>(string key, T value, JsonTypeInfo<T> jsonTypeInfo, string encryptionKey = "") where T : notnull {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(value, jsonTypeInfo);
        Upsert(key, bytes, encryptionKey);
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
                int estimatedSize = kvp.GetEstimatedSize();
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