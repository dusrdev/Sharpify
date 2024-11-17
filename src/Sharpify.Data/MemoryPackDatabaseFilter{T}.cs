using MemoryPack;

using Sharpify.Collections;

namespace Sharpify.Data;

/// <summary>
/// Provides a light database filter by type.
/// </summary>
/// <remarks>
/// Items that are upserted into the database using the filter, should not be retrieved without the filter as the key is modified.
/// </remarks>
/// <typeparam name="T"></typeparam>
public class MemoryPackDatabaseFilter<T> : IDatabaseFilter<T> where T : IMemoryPackable<T> {
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
	public MemoryPackDatabaseFilter(Database database) {
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
        return _database.TryGetValue(disposableKey.Key, encryptionKey, out value);
    }

    /// <inheritdoc />
    public bool TryGetValues(ReadOnlySpan<char> key, string encryptionKey, out T[] values) {
        using var disposableKey = DisposableKey.Create(KeyFilter, key);
        return _database.TryGetValues(disposableKey.Key, encryptionKey, out values);
    }

    /// <inheritdoc />
    public RentedBufferWriter<T> TryReadToRentedBuffer(ReadOnlySpan<char> key, string encryptionKey = "", int reservedCapacity = 0) {
        using var disposableKey = DisposableKey.Create(KeyFilter, key);
        return _database.TryReadToRentedBuffer<T>(disposableKey.Key, encryptionKey, reservedCapacity);
    }

    /// <inheritdoc />
    public bool Upsert(ReadOnlySpan<char> key, T value, string encryptionKey = "", Func<T, bool>? updateCondition = null) {
        using var disposableKey = DisposableKey.Create(KeyFilter, key);
        return _database.Upsert(disposableKey.Key, value, encryptionKey, updateCondition);
    }

    /// <inheritdoc />
    public bool UpsertMany(ReadOnlySpan<char> key, T[] values, string encryptionKey = "", Func<T[], bool>? updateCondition = null) {
        using var disposableKey = DisposableKey.Create(KeyFilter, key);
        return _database.UpsertMany(disposableKey.Key, values, encryptionKey, updateCondition);
    }

    /// <inheritdoc />
    public bool UpsertMany(ReadOnlySpan<char> key, ReadOnlySpan<T> values, string encryptionKey = "", Func<T[], bool>? updateCondition = null) {
        using var disposableKey = DisposableKey.Create(KeyFilter, key);
        return _database.UpsertMany(disposableKey.Key, values, encryptionKey, updateCondition);
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