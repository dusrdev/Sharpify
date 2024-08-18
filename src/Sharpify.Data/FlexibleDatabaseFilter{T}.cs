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
    protected virtual string AcquireKey(ReadOnlySpan<char> key) => string.Intern(KeyFilter.Concat(key));

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
    public bool ContainsKey(string key) => _database.ContainsKey(AcquireKey(key));

    /// <inheritdoc />
    public bool TryGetValue(string key, string encryptionKey, out T value) {
        if (!_database.TryGetValue(AcquireKey(key), encryptionKey, out var data)) {
            value = default!;
            return false;
        }
        var span = data.Span;
        value = T.Deserialize(in span)!;
        return true;
    }

    /// <inheritdoc />
    public bool TryGetValues(string key, string encryptionKey, out T[] values) {
        if (!_database.TryGetValue(AcquireKey(key), encryptionKey, out ReadOnlyMemory<byte> data)) {
            values = default!;
            return false;
        }
        var span = data.Span;
        values = T.DeserializeMany(in span)!;
        return true;
    }

    /// <inheritdoc />
    public RentedBufferWriter<T> TryReadToRentedBuffer(string key, string encryptionKey = "", int reservedCapacity = 0) {
        if (!_database.TryGetValue(AcquireKey(key), encryptionKey, out ReadOnlyMemory<byte> data)) {
            return new RentedBufferWriter<T>(0);
        }
        var span = data.Span;
        T[] values = T.DeserializeMany(in span)!;
        var buffer = new RentedBufferWriter<T>(values.Length + reservedCapacity);
        buffer.WriteAndAdvance(values);
        return buffer;
    }

    /// <inheritdoc />
    public void Upsert(string key, T value, string encryptionKey = "") {
        ArgumentNullException.ThrowIfNull(value, nameof(value));
        var bytes = T.Serialize(value)!;
        _database.Upsert(AcquireKey(key), bytes, encryptionKey);
    }

    /// <inheritdoc />
    public void UpsertMany(string key, T[] values, string encryptionKey = "") {
       ArgumentNullException.ThrowIfNull(values, nameof(values));
       var bytes = T.SerializeMany(values)!;
        _database.Upsert(AcquireKey(key), bytes, encryptionKey);
    }

    /// <inheritdoc />
    public void UpsertMany(string key, ReadOnlySpan<T> values, string encryptionKey = "") {
        var array = values.ToArray();
        var bytes = T.SerializeMany(array)!;
        _database.Upsert(AcquireKey(key), bytes, encryptionKey);
    }

    /// <inheritdoc />
    public bool Remove(string key) => _database.Remove(AcquireKey(key));

    /// <inheritdoc />
    public void Remove(Func<string, bool> keySelector) => _database.Remove(keySelector, KeyFilter);

    /// <inheritdoc />
    public void Serialize() => _database.Serialize();

    /// <inheritdoc />
    public ValueTask SerializeAsync(CancellationToken cancellationToken = default) => _database.SerializeAsync(cancellationToken);
}