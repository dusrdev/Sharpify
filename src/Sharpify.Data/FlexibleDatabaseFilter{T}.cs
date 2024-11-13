using System.Runtime.CompilerServices;

using Sharpify.Collections;

namespace Sharpify.Data;

/// <summary>
/// Provides a light database filter by type.
/// </summary>
/// <remarks>
/// Items that are upserted into the database using the filter, should not be retrieved without the filter as the key is modified.
/// </remarks>
/// <typeparam name="T"></typeparam>
public class FlexibleDatabaseFilter<T> : IDatabaseFilter<T> where T : IFilterable<T> {
    /// <summary>
    /// The key filter, statically created for the type.
    /// </summary>
    public static readonly string KeyFilter = $"{typeof(T).Name}:";

    /// <summary>
    /// Creates a combined key (filter) for the specified key.
    /// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected string AcquireKey(ReadOnlySpan<char> key) {
        return string.Intern(KeyFilter.Concat(key));
    }

    /// <summary>
    /// The database.
    /// </summary>
    protected readonly Database _database;

    /// <summary>
    /// Creates a new database filter.
    /// </summary>
    /// <param name="database"></param>
	public FlexibleDatabaseFilter(Database database) {
        _database = database;
    }

    /// <inheritdoc />
    public bool ContainsKey(string key) {
        return _database.ContainsKey(AcquireKey(key));
    }


    /// <inheritdoc />
    public bool TryGetValue(string key, string encryptionKey, out T value) {
        if (!_database.TryGetValue(AcquireKey(key), encryptionKey, out var data)) {
            value = default!;
            return false;
        }
        value = T.Deserialize(data.Span)!;
        return true;
    }

    /// <inheritdoc />
    public bool TryGetValues(string key, string encryptionKey, out T[] values) {
        if (!_database.TryGetValue(AcquireKey(key), encryptionKey, out ReadOnlyMemory<byte> data)) {
            values = default!;
            return false;
        }
        values = T.DeserializeMany(data.Span)!;
        return true;
    }

    /// <inheritdoc />
    public RentedBufferWriter<T> TryReadToRentedBuffer(string key, string encryptionKey = "", int reservedCapacity = 0) {
        if (!_database.TryGetValue(AcquireKey(key), encryptionKey, out ReadOnlyMemory<byte> data)) {
            return new RentedBufferWriter<T>(0);
        }
        T[] values = T.DeserializeMany(data.Span)!;
        var buffer = new RentedBufferWriter<T>(values.Length + reservedCapacity);
        buffer.WriteAndAdvance(values);
        return buffer;
    }

    /// <inheritdoc />
    public bool Upsert(string key, T value, string encryptionKey = "", Func<T, bool>? updateCondition = null) {
        if (updateCondition is not null) {
            if (TryGetValue(key, encryptionKey, out var existingValue) && !updateCondition(existingValue)) {
                return false;
            }
        }
        var bytes = T.Serialize(value)!;
        _database.Upsert(AcquireKey(key), bytes, encryptionKey);
        return true;
    }

    /// <inheritdoc />
    public bool UpsertMany(string key, T[] values, string encryptionKey = "", Func<T[], bool>? updateCondition = null) {
        ArgumentNullException.ThrowIfNull(values, nameof(values));
        if (updateCondition is not null) {
            if (TryGetValues(key, encryptionKey, out var existingValues) && !updateCondition(existingValues)) {
                return false;
            }
        }
        var bytes = T.SerializeMany(values)!;
        _database.Upsert(AcquireKey(key), bytes, encryptionKey);
        return true;
    }

    /// <inheritdoc />
    public bool UpsertMany(string key, ReadOnlySpan<T> values, string encryptionKey = "", Func<T[], bool>? updateCondition = null) {
        return UpsertMany(key, values.ToArray(), encryptionKey, updateCondition);
    }

    /// <inheritdoc />
    public bool Remove(string key) {
        return _database.Remove(AcquireKey(key));
    }


    /// <inheritdoc />
    public void Remove(Func<string, bool> keySelector) {
        _database.Remove(keySelector, KeyFilter);
    }


    /// <inheritdoc />
    public void Serialize() {
        _database.Serialize();
    }


    /// <inheritdoc />
    public ValueTask SerializeAsync(CancellationToken cancellationToken = default) {
        return _database.SerializeAsync(cancellationToken);
    }
}