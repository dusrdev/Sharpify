using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

using MemoryPack;

namespace Sharpify.Data;

public sealed partial class Database {
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
    public void Upsert(ReadOnlySpan<char> key, ReadOnlySpan<byte> value, string encryptionKey = "") {
        if (encryptionKey.Length is 0) {
            UpsertEntry(key, value.ToArray());
        } else {
            byte[] encrypted = Helper.Instance.Encrypt(value, encryptionKey);
            UpsertEntry(key, encrypted);
        }

        if (Config.TriggerUpdateEvents) {
            InvokeDataEvent(new DataChangedEventArgs {
                Key = _alternateComparer.Create(key),
                Value = value.ToArray(),
                ChangeType = DataChangeType.Upsert
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
    /// <para>
    /// This method directly inserts the array reference in to the database to reduce copying.
    /// </para>
    /// <para>
    /// If you cannot ensure that this reference doesn't change, for example if using a pooled array, use the <see cref="Upsert(ReadOnlySpan{char}, ReadOnlySpan{byte}, string)"/> method instead.
    /// </para>
    /// </remarks>
    public void Upsert(ReadOnlySpan<char> key, byte[] value, string encryptionKey = "") {
        if (encryptionKey.Length is 0) {
            UpsertEntry(key, value);
        } else {
            ReadOnlySpan<byte> valueSpan = value;
            byte[] encrypted = Helper.Instance.Encrypt(valueSpan, encryptionKey);
            UpsertEntry(key, encrypted);
        }

        if (Config.TriggerUpdateEvents) {
            InvokeDataEvent(new DataChangedEventArgs {
                Key = _alternateComparer.Create(key),
                Value = value,
                ChangeType = DataChangeType.Upsert
            });
        }
    }

    /// <summary>
    /// Upserts a value into the database using the specified key.
    /// </summary>
    /// <typeparam name="T">The type of the value being upserted.</typeparam>
    /// <param name="key">The key used to identify the value.</param>
    /// <param name="value">The value to be upserted.</param>
    /// <param name="encryptionKey">The encryption key used to encrypt the value.</param>
    /// <param name="updateCondition">a conditional check that the previously stored value must pass before being updated</param>
    /// <remarks>
    /// The upsert operation will either insert a new value if the key does not exist,
    /// or update the existing value if the key already exists.
    /// <para>
    /// Null values are disallowed and will cause an exception to be thrown.
    /// </para>
    /// </remarks>
    /// <returns>
	/// False if the previous value exists, <paramref name="updateCondition"/> is not null, and the update condition is not met, otherwise True.
	/// </returns>
    public bool Upsert<T>(ReadOnlySpan<char> key, T value, string encryptionKey = "", Func<T, bool>? updateCondition = null) where T : IMemoryPackable<T> {
        if (updateCondition is not null) {
            if (TryGetValue<T>(key, encryptionKey, out var existingValue) && !updateCondition(existingValue)) {
                return false;
            }
        }
        byte[] bytes = MemoryPackSerializer.Serialize(value, _serializer.SerializerOptions);
        Upsert(key, bytes, encryptionKey);
        return true;
    }

    /// <summary>
    /// Upserts values into the database using the specified key.
    /// </summary>
    /// <typeparam name="T">The type of the values being upserted.</typeparam>
    /// <param name="key">The key used to identify the values.</param>
    /// <param name="values">The values to be upserted.</param>
    /// <param name="encryptionKey">The encryption key used to encrypt the values.</param>
    /// <param name="updateCondition">a conditional check that the previously stored value must pass before being updated</param>
    /// <remarks>
    /// The upsert operation will either insert if the key does not exist,
    /// or update the existing values if the key already exists.
    /// <para>
    /// Null values are disallowed and will cause an exception to be thrown.
    /// </para>
    /// </remarks>
    /// <returns>
	/// False if the previous values exist, <paramref name="updateCondition"/> is not null, and the update condition is not met, otherwise True.
	/// </returns>
    public bool UpsertMany<T>(ReadOnlySpan<char> key, T[] values, string encryptionKey = "", Func<T[], bool>? updateCondition = null) where T : IMemoryPackable<T> {
        ArgumentNullException.ThrowIfNull(values, nameof(values));
        if (updateCondition is not null) {
            if (TryGetValues<T>(key, encryptionKey, out var existingValues) && !updateCondition(existingValues)) {
                return false;
            }
        }
        byte[] bytes = MemoryPackSerializer.Serialize(values, _serializer.SerializerOptions);
        Upsert(key, bytes, encryptionKey);
        return true;
    }

    /// <summary>
    /// Upserts values into the database using the specified key.
    /// </summary>
    /// <typeparam name="T">The type of the values being upserted.</typeparam>
    /// <param name="key">The key used to identify the values.</param>
    /// <param name="values">The values to be upserted.</param>
    /// <param name="encryptionKey">The encryption key used to encrypt the values.</param>
    /// <param name="updateCondition">a conditional check that the previously stored value must pass before being updated</param>
    /// <remarks>
    /// The upsert operation will either insert if the key does not exist,
    /// or update the existing values if the key already exists.
    /// <para>
    /// Null values are disallowed and will cause an exception to be thrown.
    /// </para>
    /// </remarks>
    /// <returns>
	/// False if the previous values exist, <paramref name="updateCondition"/> is not null, and the update condition is not met, otherwise True.
	/// </returns>
    public bool UpsertMany<T>(ReadOnlySpan<char> key, ReadOnlySpan<T> values, string encryptionKey = "", Func<T[], bool>? updateCondition = null) where T : IMemoryPackable<T> {
        return UpsertMany(key, values.ToArray(), encryptionKey, updateCondition);
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
    public void Upsert(ReadOnlySpan<char> key, string value, string encryptionKey = "") {
        byte[] bytes = value.Length is 0
                    ? Array.Empty<byte>()
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
    /// <param name="updateCondition">a conditional check that the previously stored value must pass before being updated</param>
    /// <remarks>
    /// <para>
    /// Null values are disallowed and will cause an exception to be thrown.
    /// </para>
    /// </remarks>
    /// <returns>
	/// False if the previous value exists, <paramref name="updateCondition"/> is not null, and the update condition is not met, otherwise True.
	/// </returns>
    public bool Upsert<T>(ReadOnlySpan<char> key, T value, JsonTypeInfo<T> jsonTypeInfo, string encryptionKey = "", Func<T, bool>? updateCondition = null) {
        if (updateCondition is not null) {
            if (!TryGetValue(key, encryptionKey, jsonTypeInfo, out var existingValue) || !updateCondition(existingValue)) {
                return false;
            }
        }
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(value, jsonTypeInfo);
        Upsert(key, bytes, encryptionKey);
        return true;
    }

    /// <summary>
    /// Adds or updates an entry to the database
    /// </summary>
    /// <param name="key">Entry key</param>
    /// <param name="value">Entry value</param>
    private void UpsertEntry(ReadOnlySpan<char> key, byte[] value) {
        // Adding values is thread safe
        if (_lookup.TryAdd(key, value)) {
            // Only if not existed before
            int estimatedSize = Helper.GetEstimatedSize(key, value);
            Interlocked.Add(ref _estimatedSize, estimatedSize);
            Interlocked.Increment(ref _updatesCount);
        } else {
            // This case requires delta of size difference to get more accurate size estimates
            var prevLength = _lookup[key]?.Length ?? 0;
            _lookup[key] = value;
            var change = value.Length - prevLength;
            Interlocked.Add(ref _estimatedSize, change);
            Interlocked.Increment(ref _updatesCount);
        }

        // sync serialization to allow concurrent writing threads (1+) to skip serialization
        // serialize will check if needed by update count
        lock (_lock) {
            Serialize();
        }
    }
}