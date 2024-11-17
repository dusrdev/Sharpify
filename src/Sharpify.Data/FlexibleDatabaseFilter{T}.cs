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
    public bool ContainsKey(ReadOnlySpan<char> key) {
        using var disposableKey = DisposableKey.Create(KeyFilter, key);
        return _database.ContainsKey(disposableKey.Key);
    }


    /// <inheritdoc />
    public bool TryGetValue(ReadOnlySpan<char> key, string encryptionKey, out T value) {
        using var disposableKey = DisposableKey.Create(KeyFilter, key);
        if (!_database.TryGetValue(disposableKey.Key, encryptionKey, out var data)) {
            value = default!;
            return false;
        }
        value = T.Deserialize(data.Span)!;
        return true;
    }

    /// <inheritdoc />
    public bool TryGetValues(ReadOnlySpan<char> key, string encryptionKey, out T[] values) {
        using var disposableKey = DisposableKey.Create(KeyFilter, key);
        if (!_database.TryGetValue(disposableKey.Key, encryptionKey, out ReadOnlyMemory<byte> data)) {
            values = default!;
            return false;
        }
        values = T.DeserializeMany(data.Span)!;
        return true;
    }

    /// <inheritdoc />
    public RentedBufferWriter<T> TryReadToRentedBuffer(ReadOnlySpan<char> key, string encryptionKey = "", int reservedCapacity = 0) {
        using var disposableKey = DisposableKey.Create(KeyFilter, key);
        if (!_database.TryGetValue(disposableKey.Key, encryptionKey, out ReadOnlyMemory<byte> data)) {
            return new RentedBufferWriter<T>(0);
        }
        T[] values = T.DeserializeMany(data.Span)!;
        var buffer = new RentedBufferWriter<T>(values.Length + reservedCapacity);
        buffer.WriteAndAdvance(values);
        return buffer;
    }

    /// <inheritdoc />
    public bool Upsert(ReadOnlySpan<char> key, T value, string encryptionKey = "", Func<T, bool>? updateCondition = null) {
        using var disposableKey = DisposableKey.Create(KeyFilter, key);
        if (updateCondition is not null) {
            if (TryGetValue(key, encryptionKey, out var existingValue) && !updateCondition(existingValue)) {
                return false;
            }
        }
        var bytes = T.Serialize(value)!;
        _database.Upsert(disposableKey.Key, bytes, encryptionKey);
        return true;
    }

    /// <inheritdoc />
    public bool UpsertMany(ReadOnlySpan<char> key, T[] values, string encryptionKey = "", Func<T[], bool>? updateCondition = null) {
        using var disposableKey = DisposableKey.Create(KeyFilter, key);
        ArgumentNullException.ThrowIfNull(values, nameof(values));
        if (updateCondition is not null) {
            if (TryGetValues(key, encryptionKey, out var existingValues) && !updateCondition(existingValues)) {
                return false;
            }
        }
        var bytes = T.SerializeMany(values)!;
        _database.Upsert(disposableKey.Key, bytes, encryptionKey);
        return true;
    }

    /// <inheritdoc />
    public bool UpsertMany(ReadOnlySpan<char> key, ReadOnlySpan<T> values, string encryptionKey = "", Func<T[], bool>? updateCondition = null) {
        using var disposableKey = DisposableKey.Create(KeyFilter, key);
        return UpsertMany(key, values.ToArray(), encryptionKey, updateCondition);
    }

    /// <inheritdoc />
    public bool Remove(ReadOnlySpan<char> key) {
        using var disposableKey = DisposableKey.Create(KeyFilter, key);
        return _database.Remove(new string(disposableKey.Key));
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